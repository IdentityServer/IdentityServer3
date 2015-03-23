/*
 * Copyright 2014, 2015 Dominick Baier, Brock Allen
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

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Text;
using FluentAssertions;
using Microsoft.Owin;
using Microsoft.Owin.Security.DataHandler;
using Microsoft.Owin.Security.Google;
using Microsoft.Owin.Testing;
using Moq;
using Owin;
using Thinktecture.IdentityServer.Core;
using Thinktecture.IdentityServer.Core.Configuration;
using Thinktecture.IdentityServer.Core.Configuration.AppBuilderExtensions;
using Thinktecture.IdentityServer.Core.Configuration.Hosting;
using Thinktecture.IdentityServer.Core.Models;
using Thinktecture.IdentityServer.Core.Services;
using Thinktecture.IdentityServer.Core.Services.InMemory;
using Thinktecture.IdentityServer.Core.ViewModels;

namespace Thinktecture.IdentityServer.Tests.Endpoints.Setup
{
    public class IdSvrHostTestBase
    {
        protected TestServer Server;
        protected HttpClient Client;
        protected IDataProtector Protector;
        protected TicketDataFormat TicketFormatter;

        protected Mock<InMemoryUserService> MockUserService;
        protected IdentityServerOptions Options;

        protected IAppBuilder AppBuilder;
        protected Action<IAppBuilder, string> OverrideIdentityProviderConfiguration { get; set; }

        protected List<Client> Clients;

        protected Action<IdentityServerOptions> ConfigureIdentityServerOptions;

        protected GoogleOAuth2AuthenticationOptions Google;
        protected GoogleOAuth2AuthenticationOptions Google2;
        protected GoogleOAuth2AuthenticationOptions HiddenGoogle;

        public IdSvrHostTestBase()
        {
            Init();
        }

        protected void Init()
        {
            Clients = TestClients.Get();
            var clientStore = new InMemoryClientStore(Clients);
            var scopeStore = new InMemoryScopeStore(TestScopes.Get());

            var factory = new IdentityServerServiceFactory
            {
                ScopeStore = new Registration<IScopeStore>(resolver => scopeStore),
                ClientStore = new Registration<IClientStore>(resolver => clientStore)
            };

            Server = TestServer.Create(app =>
            {
                AppBuilder = app;

                MockUserService = new Mock<InMemoryUserService>(TestUsers.Get()) {CallBase = true};
                factory.UserService = new Registration<IUserService>(resolver => MockUserService.Object);

                Options = TestIdentityServerOptions.Create();
                Options.Factory = factory;
                Options.AuthenticationOptions.IdentityProviders = OverrideIdentityProviderConfiguration ?? ConfigureAdditionalIdentityProviders;

                Protector = Options.DataProtector;

                if (ConfigureIdentityServerOptions != null) ConfigureIdentityServerOptions(Options);
                app.UseIdentityServer(Options);

                TicketFormatter = new TicketDataFormat(
                    new DataProtectorAdapter(Protector, Options.AuthenticationOptions.CookieOptions.Prefix + Constants.PARTIAL_SIGN_IN_AUTHENTICATION_TYPE));
            });

            Client = Server.HttpClient;
        }

        public virtual void ConfigureAdditionalIdentityProviders(IAppBuilder app, string signInAsType)
        {
            app.Use(async (ctx, next) =>
            {
                Preprocess(ctx);
                await next();
                Postprocess(ctx);
            });

            Google = new GoogleOAuth2AuthenticationOptions
            {
                AuthenticationType = "Google",
                SignInAsAuthenticationType = signInAsType,
                ClientId = "foo",
                ClientSecret = "bar"
            };
            app.UseGoogleAuthentication(Google);

            Google2 = new GoogleOAuth2AuthenticationOptions
            {
                Caption = "Google2",
                AuthenticationType = "Google2",
                SignInAsAuthenticationType = signInAsType,
                ClientId = "g2",
                ClientSecret = "g2"
            };
            app.UseGoogleAuthentication(Google2);

            HiddenGoogle = new GoogleOAuth2AuthenticationOptions
            {
                AuthenticationType = "HiddenGoogle",
                Caption = null,
                SignInAsAuthenticationType = signInAsType,
                ClientId = "baz",
                ClientSecret = "quux"
            };
            app.UseGoogleAuthentication(HiddenGoogle);
        }

        public AntiForgeryTokenViewModel Xsrf { get; set; }

        protected void ProcessXsrf(HttpResponseMessage resp)
        {
            if (resp.IsSuccessStatusCode)
            {
                var model = resp.GetModel<LoginViewModel>();
                if (model.AntiForgery != null)
                {
                    Xsrf = model.AntiForgery;
                    var cookies = resp.GetCookies().Where(x => x.Name == Xsrf.Name);
                    Client.SetCookies(cookies);
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
            return Client.GetAsync(Url(path)).Result;
        }

        protected T Get<T>(string path)
        {
            var result = Get(path);
            result.IsSuccessStatusCode.Should().BeTrue();
            return result.Content.ReadAsAsync<T>().Result;
        }

        protected NameValueCollection Map(object values)
        {
            var coll = values as NameValueCollection;
            if (coll != null) return coll;

            coll = new NameValueCollection();
            foreach (PropertyDescriptor descriptor in TypeDescriptor.GetProperties(values))
            {
                var val = descriptor.GetValue(values) ?? "";
                coll.Add(descriptor.Name, val.ToString());
            }
            return coll;
        }

        protected string ToFormBody(NameValueCollection coll)
        {
            var sb = new StringBuilder();
            foreach (var item in coll.AllKeys)
            {
                if (sb.Length > 0)
                {
                    sb.Append("&");
                }
                sb.AppendFormat("{0}={1}", item, coll[item]);
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
            var content = new StringContent(body, Encoding.UTF8, FormUrlEncodedMediaTypeFormatter.DefaultMediaType.MediaType);
            return Client.PostAsync(Url(path), content).Result;
        }

        protected HttpResponseMessage Post<T>(string path, T value)
        {
            return Client.PostAsJsonAsync(Url(path), value).Result;
        }

        protected HttpResponseMessage Put<T>(string path, T value)
        {
            return Client.PutAsJsonAsync(Url(path), value).Result;
        }

        protected HttpResponseMessage Delete(string path)
        {
            return Client.DeleteAsync(Url(path)).Result;
        }

        protected string WriteMessageToCookie<T>(T msg)
            where T : Message
        {
            var cookieStates = Client.DefaultRequestHeaders.GetCookies().SelectMany(c => c.Cookies);
            var requestCookies = cookieStates.Select(c => c.ToString()).ToArray();
            var requestHeaders = new Dictionary<string, string[]>
            {
                {"Cookie", requestCookies}
            };

            var responseHeaders = new Dictionary<string, string[]>();
            var env = new Dictionary<string, object>()
            {
                {"owin.RequestScheme", "https"},
                {"owin.RequestHeaders", requestHeaders},
                {"owin.ResponseHeaders", responseHeaders},
                {Constants.OwinEnvironment.IDENTITY_SERVER_BASE_PATH, "/"},
            };

            var ctx = new OwinContext(env);
            var signInCookie = new MessageCookie<T>(ctx, Options);
            var id = signInCookie.Write(msg);

            Client.SetCookies(responseHeaders["Set-Cookie"]);

            return id;
        }
    }
}
