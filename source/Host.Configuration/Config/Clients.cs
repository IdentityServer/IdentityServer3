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

using IdentityServer3.Core;
using IdentityServer3.Core.Models;
using System.Collections.Generic;
using System.Security.Claims;

namespace IdentityServer3.Host.Config
{
    public class Clients
    {
        public static List<Client> Get()
        {
            return new List<Client>
            {

                /////////////////////////////////////////////////////////////
                // Console Client Credentials Sample
                /////////////////////////////////////////////////////////////
                new Client
                {
                    ClientName = "Client Credentials Flow Client",
                    Enabled = true,
                    ClientId = "clientcredentials.client",
                    Flow = Flows.ClientCredentials,

                    ClientSecrets = new List<Secret>
                        {
                            new Secret("secret".Sha256()),
                            new Secret
                            {
                                Value = "61B754C541BBCFC6A45A9E9EC5E47D8702B78C29",
                                Type = Constants.SecretTypes.X509CertificateThumbprint,
                                Description = "Client Certificate"
                            },
                        },

                    AllowedScopes = new List<string>
                        {
                            "read",
                            "write"
                        },

                    Claims = new List<Claim>
                        {
                            new Claim("location", "datacenter")
                        }
                },

                /////////////////////////////////////////////////////////////
                // Console Custom Grant Type Sample
                /////////////////////////////////////////////////////////////
                new Client
                {
                    ClientName = "Custom Grant Client",
                    ClientId = "customgrant.client",
                    Flow = Flows.Custom,

                    ClientSecrets = new List<Secret>
                    {
                        new Secret("secret".Sha256())
                    },

                    AllowedScopes = new List<string>
                    {
                        "read",
                        "write"
                    },

                    AllowedCustomGrantTypes = new List<string>
                    {
                        "custom"
                    }
                },

                /////////////////////////////////////////////////////////////
                // Resource Owner Flow Samples
                /////////////////////////////////////////////////////////////
                new Client
                {
                    ClientName = "Resource Owner Flow Client",
                    ClientId = "ro.client",
                    Flow = Flows.ResourceOwner,

                    ClientSecrets = new List<Secret>
                    {
                        new Secret("secret".Sha256())
                    },

                    AllowedScopes = new List<string>
                    {
                        "openid",
                        "email",
                        "read",
                        "write",
                        "address",
                        "offline_access"
                    },

                    // used by JS resource owner sample
                    AllowedCorsOrigins = new List<string>
                    {
                        "http://localhost:13048"
                    },

                    AccessTokenType = AccessTokenType.Jwt,
                    AccessTokenLifetime = 3600,

                    // refresh token settings
                    AbsoluteRefreshTokenLifetime = 86400,
                    SlidingRefreshTokenLifetime = 43200,
                    RefreshTokenUsage = TokenUsage.OneTimeOnly,
                    RefreshTokenExpiration = TokenExpiration.Sliding
                },

                /////////////////////////////////////////////////////////////
                // JavaScript Implicit Client - OAuth only
                /////////////////////////////////////////////////////////////
                new Client
                {
                    ClientName = "JavaScript Implicit Client - Simple",
                    ClientId = "js.simple",
                    Flow = Flows.Implicit,

                    AllowedScopes = new List<string>
                    {
                        "read",
                        "write"
                    },

                    ClientUri = "https://identityserver.io",

                    RequireConsent = true,
                    AllowRememberConsent = true,

                    RedirectUris = new List<string>
                    {
                        "http://localhost:37045/index.html",
                    },
                },

                /////////////////////////////////////////////////////////////
                // JavaScript Implicit Client - Manual
                /////////////////////////////////////////////////////////////
                new Client
                {
                    ClientName = "JavaScript Implicit Client - Manual",
                    ClientId = "js.manual",
                    Flow = Flows.Implicit,

                    AllowedScopes = new List<string>
                    {
                        "openid",
                        "email",
                        "read",
                        "write"
                    },

                    ClientUri = "https://identityserver.io",

                    RequireConsent = true,
                    AllowRememberConsent = true,

                    RedirectUris = new List<string>
                    {
                        "http://localhost:37046/index.html",
                    },

                    AllowedCorsOrigins = new List<string>
                    {
                        "http://localhost:37046"
                    }
                },

                /////////////////////////////////////////////////////////////
                // JavaScript Implicit Client - TokenManager
                /////////////////////////////////////////////////////////////
                new Client
                {
                    ClientName = "JavaScript Implicit Client - TokenManager",
                    ClientId = "js.tokenmanager",
                    Flow = Flows.Implicit,

                    AllowedScopes = new List<string>
                    {
                        Constants.StandardScopes.OpenId,
                        Constants.StandardScopes.Profile,
                        Constants.StandardScopes.Email,
                        Constants.StandardScopes.Roles,
                        "read",
                        "write"
                    },

                    ClientUri = "https://identityserver.io",

                    RequireConsent = true,
                    AllowRememberConsent = true,

                    RedirectUris = new List<string>
                    {
                        "http://localhost:21575/index.html",
                        "http://localhost:21575/silent_renew.html",
                        "http://localhost:21575/callback.html",
                        "http://localhost:21575/frame.html",
                        "http://localhost:21575/popup.html",
                    },

                    PostLogoutRedirectUris = new List<string>
                    {
                        "http://localhost:21575/index.html",
                    },

                    AllowedCorsOrigins = new List<string>
                    {
                        "http://localhost:21575",
                    },

                    AccessTokenLifetime = 3600,
                    AccessTokenType = AccessTokenType.Jwt
                },

                /////////////////////////////////////////////////////////////
                // MVC CodeFlowClient Manual
                /////////////////////////////////////////////////////////////
                new Client
                {
                    ClientName = "Code Flow Client Demo",
                    ClientId = "codeclient",
                    Flow = Flows.AuthorizationCode,

                    ClientSecrets = new List<Secret>
                    {
                        new Secret("secret".Sha256())
                    },

                    RequireConsent = true,
                    AllowRememberConsent = true,

                    ClientUri = "https://identityserver.io",

                    RedirectUris = new List<string>
                    {
                        "https://localhost:44312/callback",
                    },

                    AllowedScopes = new List<string>
                    {
                        Constants.StandardScopes.OpenId,
                        Constants.StandardScopes.Profile,
                        Constants.StandardScopes.Email,
                        Constants.StandardScopes.Roles,
                        Constants.StandardScopes.OfflineAccess,
                        "read",
                        "write"
                    },

                    AccessTokenType = AccessTokenType.Reference,
                },

                /////////////////////////////////////////////////////////////
                // MVC No Library Client
                /////////////////////////////////////////////////////////////
                new Client
                {
                    ClientName = "OpenID Connect without Client Library Sample",
                    ClientId = "nolib.client",
                    Flow = Flows.Implicit,

                    AllowedScopes = new List<string>
                    {
                        Constants.StandardScopes.OpenId,
                        Constants.StandardScopes.Profile,
                        Constants.StandardScopes.Email,
                        Constants.StandardScopes.Roles,
                        Constants.StandardScopes.Address,
                        "read",
                        "write"
                    },

                    ClientUri = "https://identityserver.io",

                    RequireConsent = true,
                    AllowRememberConsent = true,

                    RedirectUris = new List<string>
                    {
                        "http://localhost:11716/account/signInCallback",
                    },
                },

                /////////////////////////////////////////////////////////////
                // MVC OWIN Hybrid Client
                /////////////////////////////////////////////////////////////
                new Client
                {
                    ClientName = "MVC OWIN Hybrid Client",
                    ClientId = "mvc.owin.hybrid",
                    Flow = Flows.Hybrid,
                    AllowAccessTokensViaBrowser = false,

                    ClientSecrets = new List<Secret>
                    {
                        new Secret("secret".Sha256())
                    },

                    AllowedScopes = new List<string>
                    {
                        Constants.StandardScopes.OpenId,
                        Constants.StandardScopes.Profile,
                        Constants.StandardScopes.Email,
                        Constants.StandardScopes.Roles,
                        Constants.StandardScopes.OfflineAccess,
                        "read",
                        "write"
                    },

                    ClientUri = "https://identityserver.io",

                    RequireConsent = false,
                    AccessTokenType = AccessTokenType.Reference,

                    RedirectUris = new List<string>
                    {
                        "https://localhost:44300/"
                    },

                    PostLogoutRedirectUris = new List<string>
                    {
                        "https://localhost:44300/"
                    },

                    LogoutUri = "https://localhost:44300/Home/OidcSignOut",
                    LogoutSessionRequired = true
                },

                
                /////////////////////////////////////////////////////////////
                // MVC OWIN Implicit Client
                /////////////////////////////////////////////////////////////
                new Client
                {
                    ClientName = "MVC OWIN Implicit Client",
                    ClientId = "mvc.owin.implicit",
                    Flow = Flows.Implicit,
                    AllowAccessTokensViaBrowser = false,

                    AllowedScopes = new List<string>
                    {
                        Constants.StandardScopes.OpenId,
                        Constants.StandardScopes.Profile,
                        Constants.StandardScopes.Email,
                        Constants.StandardScopes.Roles,
                        Constants.StandardScopes.Address,
                    },

                    ClientUri = "https://identityserver.io",

                    RequireConsent = true,
                    AllowRememberConsent = true,

                    RedirectUris = new List<string>
                    {
                        "https://localhost:44301/"
                    },

                    LogoutUri = "https://localhost:44301/Home/SignoutCleanup",
                    LogoutSessionRequired = true,
                },

                /////////////////////////////////////////////////////////////
                // WebForms OWIN Implicit Client
                /////////////////////////////////////////////////////////////
                new Client
                {
                    ClientName = "WebForms OWIN Implicit Client",
                    ClientId = "webforms.owin.implicit",
                    Flow = Flows.Implicit,

                    AllowedScopes = new List<string>
                    {
                        Constants.StandardScopes.OpenId,
                        Constants.StandardScopes.Profile,
                        Constants.StandardScopes.Email,
                        Constants.StandardScopes.Roles,
                        Constants.StandardScopes.Address,
                    },

                    ClientUri = "https://identityserver.io",

                    RequireConsent = true,
                    AllowRememberConsent = true,

                    RedirectUris = new List<string>
                    {
                        "http://localhost:5969/"
                    },
                    PostLogoutRedirectUris = new List<string>
                    {
                        "http://localhost:5969/"
                    }
                },

                /////////////////////////////////////////////////////////////
                // WPF WebView Client Sample
                /////////////////////////////////////////////////////////////
                new Client
                {
                    ClientName = "WPF WebView Client Sample",
                    ClientId = "wpf.webview.client",
                    Flow = Flows.Implicit,

                    AllowedScopes = new List<string>
                    {
                        Constants.StandardScopes.OpenId,
                        Constants.StandardScopes.Profile,
                        Constants.StandardScopes.Email,
                        Constants.StandardScopes.Roles,
                        Constants.StandardScopes.Address,
                        "read",
                        "write"
                    },

                    ClientUri = "https://identityserver.io",

                    RequireConsent = true,
                    AllowRememberConsent = true,

                    RedirectUris = new List<string>
                    {
                        "oob://localhost/wpf.webview.client",
                    },
                },

                /////////////////////////////////////////////////////////////
                // WPF Client with Hybrid Flow and PKCE
                /////////////////////////////////////////////////////////////
                new Client
                {
                    ClientName = "WPF Client with Hybrid Flow and PKCE",
                    ClientId = "wpf.hybrid",
                    Flow = Flows.HybridWithProofKey,

                    ClientSecrets = new List<Secret>
                    {
                        new Secret("secret".Sha256())
                    },

                    RedirectUris = new List<string>
                    {
                        "http://localhost/wpf.hybrid"
                    },

                    AllowedScopes = new List<string>
                    {
                        StandardScopes.OpenId.Name,
                        StandardScopes.Profile.Name,
                        "read", "write"
                    },

                    AccessTokenType = AccessTokenType.Reference
                },

                /////////////////////////////////////////////////////////////
                // WPF Client with Hybrid Flow and PKCE and PoP
                /////////////////////////////////////////////////////////////
                new Client
                {
                    ClientName = "WPF Client with Hybrid Flow and PKCE and PoP",
                    ClientId = "wpf.hybrid.pop",
                    Flow = Flows.HybridWithProofKey,

                    ClientSecrets = new List<Secret>
                    {
                        new Secret("secret".Sha256())
                    },

                    RedirectUris = new List<string>
                    {
                        "http://localhost/wpf.hybrid.pop"
                    },

                    AllowedScopes = new List<string>
                    {
                        StandardScopes.OpenId.Name,
                        StandardScopes.Profile.Name,
                        StandardScopes.OfflineAccess.Name,
                        "read", "write"
                    },

                    AccessTokenType = AccessTokenType.Reference
                },

                /////////////////////////////////////////////////////////////
                // UWP OIDC Client
                /////////////////////////////////////////////////////////////
                new Client
                {
                    ClientName = "UWP OIDC Client",
                    ClientId = "uwp",
                    ClientSecrets = new List<Secret>
                    {
                        new Secret("secret".Sha256())
                    },

                    Flow = Flows.HybridWithProofKey,

                    RedirectUris = new List<string>
                    {
                        "ms-app://s-1-15-2-491127476-3924255528-3585180829-1321445252-2746266865-3272304314-3346717936/"
                    },
                    PostLogoutRedirectUris = new List<string>
                    {
                        "ms-app://s-1-15-2-491127476-3924255528-3585180829-1321445252-2746266865-3272304314-3346717936/"
                    },
                    AllowedScopes = new List<string>
                    {
                        "openid", "profile", "write"
                    },

                    AccessTokenType = AccessTokenType.Reference
                },
            };
        }
    }
}