﻿/*
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

using Host.Configuration;
using Host.Configuration.Extensions;
using IdentityServer3.Core.Configuration;
using IdentityServer3.Core.Extensions;
using IdentityServer3.Host.Config;
using Microsoft.IdentityModel.Protocols;
using Microsoft.Owin;
using Microsoft.Owin.Security.Facebook;
using Microsoft.Owin.Security.Google;
using Microsoft.Owin.Security.OpenIdConnect;
using Microsoft.Owin.Security.Twitter;
using Microsoft.Owin.Security.WsFederation;
using System;
using System.Threading.Tasks;

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

                factory.AddCustomGrantValidators();
                factory.AddCustomTokenResponseGenerator();

                factory.ConfigureClientStoreCache();
                factory.ConfigureScopeStoreCache();
                factory.ConfigureUserServiceCache();

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
                        EnableAutoCallbackForFederatedSignout = true,
                        CookieOptions = new IdentityServer3.Core.Configuration.CookieOptions { 
                            SuppressSameSiteNoneCookiesCallback = env =>
                            {
                                var context = new OwinContext(env);
                                var userAgent = context.Request.Headers["User-Agent"].ToString();

                                // Cover all iOS based browsers here. This includes:
                                // - Safari on iOS 12 for iPhone, iPod Touch, iPad
                                // - WkWebview on iOS 12 for iPhone, iPod Touch, iPad
                                // - Chrome on iOS 12 for iPhone, iPod Touch, iPad
                                // All of which are broken by SameSite=None, because they use the iOS 
                                // networking stack.
                                if (userAgent.Contains("CPU iPhone OS 12") ||
                                    userAgent.Contains("iPad; CPU OS 12"))
                                {
                                    return true;
                                }

                                // Cover Mac OS X based browsers that use the Mac OS networking stack. 
                                // This includes:
                                // - Safari on Mac OS X.
                                // This does not include:
                                // - Chrome on Mac OS X
                                // Because they do not use the Mac OS networking stack.
                                if (userAgent.Contains("Macintosh; Intel Mac OS X 10_14") &&
                                    userAgent.Contains("Version/") && userAgent.Contains("Safari"))
                                {
                                    return true;
                                }

                                // Cover Chrome 50-69, because some versions are broken by SameSite=None, 
                                // and none in this range require it.
                                // Note: this covers some pre-Chromium Edge versions, 
                                // but pre-Chromium Edge does not require SameSite=None.
                                if (userAgent.Contains("Chrome/5") || userAgent.Contains("Chrome/6"))
                                {
                                    return true;
                                }

                                return false;
                            }
                        }
                    },
                };

                coreApp.UseIdentityServer(idsrvOptions);
            });

            return app;
        }

        public static void ConfigureIdentityProviders(IAppBuilder app, string signInAsType)
        {
            var google = new GoogleOAuth2AuthenticationOptions
            {
                AuthenticationType = "Google",
                Caption = "Google",
                SignInAsAuthenticationType = signInAsType,

                ClientId = "767400843187-8boio83mb57ruogr9af9ut09fkg56b27.apps.googleusercontent.com",
                ClientSecret = "5fWcBT0udKY7_b6E3gEiJlze"
            };
            app.UseGoogleAuthentication(google);

            var fb = new FacebookAuthenticationOptions
            {
                AuthenticationType = "Facebook",
                Caption = "Facebook",
                SignInAsAuthenticationType = signInAsType,

                AppId = "676607329068058",
                AppSecret = "9d6ab75f921942e61fb43a9b1fc25c63"
            };
            app.UseFacebookAuthentication(fb);

            var twitter = new TwitterAuthenticationOptions
            {
                AuthenticationType = "Twitter",
                Caption = "Twitter",
                SignInAsAuthenticationType = signInAsType,

                ConsumerKey = "N8r8w7PIepwtZZwtH066kMlmq",
                ConsumerSecret = "df15L2x6kNI50E4PYcHS0ImBQlcGIt6huET8gQN41VFpUCwNjM"
            };
            app.UseTwitterAuthentication(twitter);

            var aad = new OpenIdConnectAuthenticationOptions
            {
                AuthenticationType = "aad",
                Caption = "Azure AD",
                SignInAsAuthenticationType = signInAsType,

                Authority = "https://login.windows.net/4ca9cb4c-5e5f-4be9-b700-c532992a3705",
                ClientId = "96e3c53e-01cb-4244-b658-a42164cb67a9",
                RedirectUri = "https://localhost:44333/core/aadcb",
            };
            app.UseSameSiteOpenIdConnectAuthentication(aad);


            // workaround for https://katanaproject.codeplex.com/workitem/409
            var metadataAddress = "https://adfs.leastprivilege.vm/federationmetadata/2007-06/federationmetadata.xml";
            var manager = new SyncConfigurationManager(new ConfigurationManager<WsFederationConfiguration>(metadataAddress));

            var adfs = new WsFederationAuthenticationOptions
            {
                AuthenticationType = "adfs",
                Caption = "ADFS",
                SignInAsAuthenticationType = signInAsType,
                CallbackPath = new PathString("/core/adfs"),

                ConfigurationManager = manager,
                Wtrealm = "urn:idsrv3"
            };
            app.UseWsFederationAuthentication(adfs);

            var was = new WsFederationAuthenticationOptions
            {
                AuthenticationType = "was",
                Caption = "Windows",
                SignInAsAuthenticationType = signInAsType,
                CallbackPath = new PathString("/core/was"),

                MetadataAddress = "https://localhost:44350",
                Wtrealm = "urn:idsrv3"
            };
            app.UseWsFederationAuthentication(was);
        }
    }
}