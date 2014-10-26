/*
 * Copyright 2014 Dominick Baier, Brock Allen
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *   http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */
using Microsoft.Owin;
using Microsoft.Owin.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Owin;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Text;
using Thinktecture.IdentityServer.Core;
using Thinktecture.IdentityServer.Core.Configuration;
using Thinktecture.IdentityServer.Core.Configuration.Hosting;
using Thinktecture.IdentityServer.Core.Logging;
using Thinktecture.IdentityServer.Core.Services;
using Thinktecture.IdentityServer.Core.Services.InMemory;
using Thinktecture.IdentityServer.Core.ViewModels;

namespace Thinktecture.IdentityServer.Tests
{
    public class IdSvrHostTestBase
    {
        protected TestServer server;
        protected HttpClient client;
        protected IDataProtector protector;
        protected Microsoft.Owin.Security.DataHandler.TicketDataFormat ticketFormatter;

        protected Mock<InMemoryUserService> mockUserService;
        protected IdentityServerOptions options;

        public TestContext TestContext { get; set; }
        protected IAppBuilder appBuilder;
        protected Action<IAppBuilder, string> OverrideIdentityProviderConfiguration { get; set; }

        [TestInitialize]
        public void Init()
        {
            LogProvider.SetCurrentLogProvider(new DiagnosticsTraceLogProvider());
            // white space between unit tests
            LogProvider.GetLogger("").Debug("----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------");
            LogProvider.GetLogger("").Debug("UNIT TEST: " + TestContext.TestName);
            LogProvider.GetLogger("").Debug("----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------");
            
            server = TestServer.Create(app =>
            {
                appBuilder = app;
                var factory = TestFactory.Create();

                mockUserService = new Mock<InMemoryUserService>(TestUsers.Get());
                mockUserService.CallBase = true;
                factory.UserService = Registration.RegisterFactory<IUserService>(() => mockUserService.Object);

                options = TestIdentityServerOptions.Create();
                options.Factory = factory;
                options.AuthenticationOptions.IdentityProviders = OverrideIdentityProviderConfiguration ?? ConfigureAdditionalIdentityProviders;
                
                protector = options.DataProtector;
                
                app.UseIdentityServer(options);

                ticketFormatter = new Microsoft.Owin.Security.DataHandler.TicketDataFormat(
                    new DataProtectorAdapter(protector, options.AuthenticationOptions.CookieOptions.Prefix + Constants.PartialSignInAuthenticationType));
            });

            client = server.HttpClient;
        }

        public virtual void ConfigureAdditionalIdentityProviders(IAppBuilder app, string signInAsType)
        {
            app.Use(async (ctx, next) =>
            {
                Preprocess(ctx);
                await next();
                Postprocess(ctx);
            });

            var google = new Microsoft.Owin.Security.Google.GoogleOAuth2AuthenticationOptions
            {
                AuthenticationType = "Google",
                SignInAsAuthenticationType = signInAsType,
                ClientId = "foo",
                ClientSecret = "bar"
            };
            app.UseGoogleAuthentication(google);
        }

        public AntiForgeryHiddenInputViewModel Xsrf { get; set; }

        protected void ProcessXsrf(HttpResponseMessage resp)
        {
            if (resp.IsSuccessStatusCode)
            {
                var model = resp.GetModel<LoginViewModel>();
                if (model.AntiForgery != null)
                {
                    this.Xsrf = model.AntiForgery;
                    var cookies = resp.GetCookies().Where(x => x.Name == Xsrf.Name);
                    client.SetCookies(cookies);
                }
            }
        }

        protected virtual void Preprocess(IOwinContext ctx)
        {
        }
        protected virtual void Postprocess(IOwinContext ctx)
        {
        }

        protected string Url(string path)
        {
            if (path.StartsWith("http")) return path;

            if (path.StartsWith("/")) path = path.Substring(1);
            return "https://localhost:3333/" + path;
        }
        protected HttpResponseMessage Get(string path)
        {
            return client.GetAsync(Url(path)).Result;
        }

        protected T Get<T>(string path)
        {
            var result = Get(path);
            Assert.IsTrue(result.IsSuccessStatusCode);
            return result.Content.ReadAsAsync<T>().Result;
        }

        protected NameValueCollection Map(object values)
        {
            var coll = values as NameValueCollection;
            if (coll != null) return coll;

            coll = new NameValueCollection();
            foreach (PropertyDescriptor descriptor in TypeDescriptor.GetProperties(values))
            {
                var val = descriptor.GetValue(values);
                if (val == null) val = "";
                coll.Add(descriptor.Name, val.ToString());
            }
            return coll;
        }

        protected string ToFormBody(NameValueCollection coll)
        {
            var sb = new StringBuilder();
            foreach(var item in coll.AllKeys)
            {
                if (sb.Length > 0)
                {
                    sb.Append("&");
                }
                sb.AppendFormat("{0}={1}", item, coll[item].ToString());
            }
            return sb.ToString();
        }

        private NameValueCollection MapAndAddXsrf(object value)
        {
            var coll = Map(value);
            if (Xsrf != null)
            {
                coll.Add(Xsrf.Name, Xsrf.Value);
            }
            return coll;
        }

        protected HttpResponseMessage PostForm(string path, object value, bool includeCsrf = true)
        {
            var form = includeCsrf ? MapAndAddXsrf(value) : Map(value);
            var body = ToFormBody(form);
            var content = new StringContent(body, System.Text.Encoding.UTF8, FormUrlEncodedMediaTypeFormatter.DefaultMediaType.MediaType);
            return client.PostAsync(Url(path), content).Result;
        }

        protected HttpResponseMessage Post<T>(string path, T value)
        {
            return client.PostAsJsonAsync(Url(path), value).Result;
        }
        
        protected HttpResponseMessage Put<T>(string path, T value)
        {
            return client.PutAsJsonAsync(Url(path), value).Result;
        }

        protected HttpResponseMessage Delete(string path)
        {
            return client.DeleteAsync(Url(path)).Result;
        }

        protected string WriteMessageToCookie<T>(T msg)
            where T : class
        {
            var headers = new Dictionary<string, string[]>();
            var env = new Dictionary<string, object>()
            {
                {"owin.RequestScheme", "https"},
                {"owin.ResponseHeaders", headers},
                {Constants.OwinEnvironment.IdentityServerBasePath, "/"},
            };

            var ctx = new OwinContext(env);
            var signInCookie = new MessageCookie<T>(ctx, this.options);
            var id = signInCookie.Write(msg);

            client.SetCookies(headers["Set-Cookie"]);

            return id;
        }
    }
}
