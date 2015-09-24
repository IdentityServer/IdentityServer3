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

using FluentAssertions;
using IdentityServer3.Core;
using IdentityServer3.Core.Configuration;
using IdentityServer3.Core.Configuration.Hosting;
using IdentityServer3.Core.Models;
using IdentityServer3.Core.Services;
using IdentityServer3.Core.Services.InMemory;
using IdentityServer3.Core.ViewModels;
using Microsoft.Owin;
using Microsoft.Owin.Security.DataHandler;
using Microsoft.Owin.Security.Google;
using Microsoft.Owin.Testing;
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

namespace IdentityServer3.Tests.Endpoints
{
    public class IdSvrHostTestBase
    {
        protected TestServer server;
        protected HttpClient client;
        protected IDataProtector protector;
        protected TicketDataFormat ticketFormatter;

        protected Mock<InMemoryUserService> mockUserService;
        protected IdentityServerOptions options;

        protected IAppBuilder appBuilder;
        protected Action<IAppBuilder, string> OverrideIdentityProviderConfiguration { get; set; }

        protected List<Client> clients;

        protected Action<IdentityServerOptions> ConfigureIdentityServerOptions;

        protected GoogleOAuth2AuthenticationOptions google;
        protected GoogleOAuth2AuthenticationOptions google2;
        protected GoogleOAuth2AuthenticationOptions hiddenGoogle;

        public IdSvrHostTestBase()
        {
            Init();
        }

        protected void Init()
        {
            clients = TestClients.Get();
            var clientStore = new InMemoryClientStore(clients);
            var scopeStore = new InMemoryScopeStore(TestScopes.Get());

            var factory = new IdentityServerServiceFactory
            {
                ScopeStore = new Registration<IScopeStore>((resolver) => scopeStore),
                ClientStore = new Registration<IClientStore>((resolver) => clientStore)
            };

            server = TestServer.Create(app =>
            {
                appBuilder = app;

                mockUserService = new Mock<InMemoryUserService>(TestUsers.Get());
                mockUserService.CallBase = true;
                factory.UserService = new Registration<IUserService>((resolver) => mockUserService.Object);

                options = TestIdentityServerOptions.Create();
                options.Factory = factory;
                options.AuthenticationOptions.IdentityProviders = OverrideIdentityProviderConfiguration ?? ConfigureAdditionalIdentityProviders;

                protector = options.DataProtector;

                if (ConfigureIdentityServerOptions != null) ConfigureIdentityServerOptions(options);
                app.UseIdentityServer(options);

                ticketFormatter = new TicketDataFormat(
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

            google = new GoogleOAuth2AuthenticationOptions
            {
                AuthenticationType = "Google",
                SignInAsAuthenticationType = signInAsType,
                ClientId = "foo",
                ClientSecret = "bar"
            };
            app.UseGoogleAuthentication(google);

            google2 = new GoogleOAuth2AuthenticationOptions
            {
                Caption = "Google2",
                AuthenticationType = "Google2",
                SignInAsAuthenticationType = signInAsType,
                ClientId = "g2",
                ClientSecret = "g2"
            };
            app.UseGoogleAuthentication(google2);

            hiddenGoogle = new GoogleOAuth2AuthenticationOptions
            {
                AuthenticationType = "HiddenGoogle",
                Caption = null,
                SignInAsAuthenticationType = signInAsType,
                ClientId = "baz",
                ClientSecret = "quux"
            };
            app.UseGoogleAuthentication(hiddenGoogle);
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
                var val = descriptor.GetValue(values);
                if (val == null) val = "";
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
            var content = new StringContent(body, Encoding.UTF8, FormUrlEncodedMediaTypeFormatter.DefaultMediaType.MediaType);
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
            where T : Message
        {
            var cookieStates = client.DefaultRequestHeaders.GetCookies().SelectMany(c => c.Cookies);
            var requestCookies = cookieStates.Select(c => c.ToString()).ToArray();
            var request_headers = new Dictionary<string, string[]>
            {
                {"Cookie", requestCookies}
            };

            var response_headers = new Dictionary<string, string[]>();
            var env = new Dictionary<string, object>()
            {
                {"owin.RequestScheme", "https"},
                {"owin.RequestHeaders", request_headers},
                {"owin.ResponseHeaders", response_headers},
                {Constants.OwinEnvironment.IdentityServerBasePath, "/"},
            };

            var ctx = new OwinContext(env);
            var signInCookie = new MessageCookie<T>(ctx, options);
            var id = signInCookie.Write(msg);

            client.SetCookies(response_headers["Set-Cookie"]);

            return id;
        }
    }
}
