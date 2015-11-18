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

using IdentityServer3.Core.Configuration;
using IdentityServer3.Core.Extensions;
using IdentityServer3.Core.Services;
using IdentityServer3.Core.Services.Default;
using IdentityServer3.Host.Config;
using System.Linq;
using Microsoft.Owin;
using Microsoft.Owin.Security.Facebook;
using Microsoft.Owin.Security.Google;
using Microsoft.Owin.Security.OpenIdConnect;
using Microsoft.Owin.Security.Twitter;
using Microsoft.Owin.Security.WsFederation;
using System.Threading.Tasks;
using System.Security.Claims;

namespace Owin
{
    public static class IdentityServerExtension
    {
        public static IAppBuilder UseIdentityServer(this IAppBuilder app)
        {
            // uncomment to enable HSTS headers for the host
            // see: https://developer.mozilla.org/en-US/docs/Web/Security/HTTP_strict_transport_security
            //app.UseHsts();

            app.Map("/core", coreApp =>
            {
                var factory = new IdentityServerServiceFactory()
                    .UseInMemoryUsers(Users.Get())
                    .UseInMemoryClients(Clients.Get())
                    .UseInMemoryScopes(Scopes.Get());

                factory.CustomGrantValidators.Add(
                    new Registration<ICustomGrantValidator>(typeof(CustomGrantValidator)));
                factory.CustomGrantValidators.Add(
                    new Registration<ICustomGrantValidator>(typeof(AnotherCustomGrantValidator)));

                factory.ConfigureClientStoreCache();
                factory.ConfigureScopeStoreCache();
                factory.ConfigureUserServiceCache();

                factory.TokenSigningService = new Registration<ITokenSigningService, EnhancedDefaultTokenSigningService>();

                var idsrvOptions = new IdentityServerOptions
                {
                    Factory = factory,
                    SigningCertificate = Cert.Load(),

                    Endpoints = new EndpointOptions
                    {
                        // replaced by the introspection endpoint in v2.2
                        EnableAccessTokenValidationEndpoint = false
                    },

                    AuthenticationOptions = new AuthenticationOptions
                    {
                        IdentityProviders = ConfigureIdentityProviders,
                        //EnablePostSignOutAutoRedirect = true
                    },

                    //LoggingOptions = new LoggingOptions
                    //{
                    //    EnableKatanaLogging = true
                    //},

                    //EventsOptions = new EventsOptions
                    //{
                    //    RaiseFailureEvents = true,
                    //    RaiseInformationEvents = true,
                    //    RaiseSuccessEvents = true,
                    //    RaiseErrorEvents = true
                    //}
                };

                coreApp.UseIdentityServer(idsrvOptions);
                coreApp.Map("/signoutcallback", cb =>
                {
                    cb.Run(async ctx =>
                    {
                        var state = ctx.Request.Cookies["state"];
                        await ctx.Environment.RenderLoggedOutViewAsync(state);
                        ctx.Response.Cookies.Delete("state");
                    });
                });
            });

            return app;
        }

        public static void ConfigureIdentityProviders(IAppBuilder app, string signInAsType)
        {
            var aad = new OpenIdConnectAuthenticationOptions
            {
                AuthenticationType = "aad",
                Caption = "IdSvr-IdP",
                SignInAsAuthenticationType = signInAsType,
                ResponseType = "id_token",

                Authority = "https://localhost:44305/core",
                ClientId = "gateway",
                RedirectUri = "https://localhost:44333/core",
                Notifications = new OpenIdConnectAuthenticationNotifications
                {
                    SecurityTokenValidated = n =>
                    {
                        n.AuthenticationTicket.Identity.AddClaim(new Claim("id_token", n.ProtocolMessage.IdToken));
                        return Task.FromResult(0);
                    },
                    RedirectToIdentityProvider = n =>
                    {
                        if (n.ProtocolMessage.RequestType == Microsoft.IdentityModel.Protocols.OpenIdConnectRequestType.LogoutRequest)
                        {
                            var id_token_claim = n.OwinContext.Authentication.User.Claims.FirstOrDefault(x => x.Type == "id_token");
                            if (id_token_claim != null)
                            {
                                n.ProtocolMessage.IdTokenHint = id_token_claim.Value;

                                var signOutMessageId = n.OwinContext.Environment.GetSignOutMessageId();
                                if (signOutMessageId != null)
                                {
                                    n.OwinContext.Response.Cookies.Append("state", signOutMessageId);
                                }
                            }
                        }
                        return Task.FromResult(0);
                    }
                }
            };

            app.UseOpenIdConnectAuthentication(aad);
        }
    }
}