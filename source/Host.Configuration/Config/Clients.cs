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
                new Client
                {
                    ClientName = "Code Flow Client Demo",
                    Enabled = true,

                    ClientId = "codeclient",
                    ClientSecrets = new List<Secret>
                    { 
                        new Secret("secret".Sha256())
                    },

                    Flow = Flows.AuthorizationCode,
                    
                    RequireConsent = true,
                    AllowRememberConsent = true,
                    
                    ClientUri = "https://identityserver.io",
                    
                    RedirectUris = new List<string>
                    {
                        // MVC code client manual
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

                new Client
                {
                    ClientName = "Implicit Client Demo",
                    Enabled = true,

                    ClientId = "implicitclient",
                    ClientSecrets = new List<Secret>
                    { 
                        new Secret("secret".Sha256())
                    },

                    Flow = Flows.Implicit,
                    
                    AllowedScopes = new List<string>
                    {
                        Constants.StandardScopes.OpenId,
                        Constants.StandardScopes.Profile,
                        Constants.StandardScopes.Email,
                        Constants.StandardScopes.Roles,
                        Constants.StandardScopes.Address,
                        Constants.StandardScopes.OfflineAccess,
                        "read",
                        "write"
                    },
                    
                    ClientUri = "https://identityserver.io",

                    RequireConsent = true,
                    AllowRememberConsent = true,

                    RedirectUris = new List<string>
                    {
                        // JS client with popup login dialog
                        "http://localhost:37045/index.html",
                        "http://localhost:37046/index.html",
                        "http://localhost:37047/index.html",
                        "http://localhost:37047/callback.html",
                        "http://localhost:37047/modal.html",
                        "http://localhost:37047/popup.html",
                        "http://localhost:37047/frame.html",

                        // "simple JS client"
                        "https://localhost:44331/Home/Callback",

                        // OAuthJS client
                        "http://localhost:23453/callback.html",
                        "http://localhost:23453/frame.html",
                        "http://localhost:23453/modal.html",
                        "http://localhost:23453/popup.html",

                        // WPF client
                        "oob://localhost/wpfclient",
                        
                        // WinRT client
                        "ms-app://s-1-15-2-1677770454-1667073387-2045065244-1646983296-4049597744-3433330513-3528227871/",

                        // JavaScript client
                        "http://localhost:21575/index.html",
                        "http://localhost:21575/silent_renew.html",

                        // MVC form post sample
                        "http://localhost:11716/account/signInCallback",

                        // OWIN middleware client
                        "http://localhost:2671/",
                        "https://localhost:44301/"
                    },

                    PostLogoutRedirectUris = new List<string>
                    {
                        "http://localhost:23453/index.html",
                        "http://localhost:21575/index.html",
                        "http://localhost:37045/index.html",
                        "http://localhost:37046/index.html",
                        "http://localhost:37047/index.html"
                    },

                    AllowedCorsOrigins = new List<string>{
                        "http://localhost:21575",
                        "http://localhost:37045",
                        "http://localhost:37046",
                        "http://localhost:37047",
                        "http://localhost:23453"
                    },

                    LogoutUri = "https://localhost:44301/Home/SignoutCleanup",
                    LogoutSessionRequired = true,
                    
                    IdentityTokenLifetime = 360,
                    AccessTokenLifetime = 3600,
                    AccessTokenType = AccessTokenType.Reference
                },

                new Client
                {
                    ClientName = "Hybrid Native Client Demo",
                    Enabled = true,
                    ClientId = "hybridclient",
                    ClientSecrets = new List<Secret>
                    { 
                        new Secret("secret".Sha256())
                    },

                    Flow = Flows.Hybrid,
                    
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

                    RequireConsent = true,
                    AllowRememberConsent = true,
                    
                    RedirectUris = new List<string>
                    {
                        "oob://localhost/wpfclient"
                    }
                },

                new Client
                {
                    ClientName = "Katana Hybrid Client Demo",
                    Enabled = true,
                    ClientId = "katanaclient",
                    ClientSecrets = new List<Secret>
                    { 
                        new Secret("secret".Sha256())
                    },

                    Flow = Flows.Hybrid,
                    
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
                        "http://localhost:2672/",
                        "https://localhost:44300/"
                    },

                    PostLogoutRedirectUris = new List<string>
                    {
                        "http://localhost:2672/",
                        "https://localhost:44300/"
                    },

                    LogoutUri = "https://localhost:44300/Home/OidcSignOut",
                    LogoutSessionRequired = true
                },

                new Client
                {
                    ClientName = "Client Credentials Flow Client",
                    Enabled = true,
                    ClientId = "client",
                    
                    ClientSecrets = new List<Secret>
                    { 
                        new Secret("secret".Sha256())
                    },

                    Flow = Flows.ClientCredentials,

                    AllowedScopes = new List<string> 
                    {
                        "read", 
                        "write"
                    },
                    
                    Claims = new List<Claim>
                    {
                        new Claim("client_type", "headless")
                    },
                    PrefixClientClaims = false
                },

                new Client
                {
                    ClientName = "Client Credentials Flow Client with Client Certificate",
                    Enabled = true,
                    
                    ClientId = "certclient",
                    ClientSecrets = new List<Secret>
                    { 
                        new Secret
                        {
                            Value = "61B754C541BBCFC6A45A9E9EC5E47D8702B78C29",
                            Type = Constants.SecretTypes.X509CertificateThumbprint,
                        }
                    },

                    Flow = Flows.ClientCredentials,
                    
                    AllowedScopes = new List<string> 
                    {
                        "read", 
                        "write"
                    },

                    Claims = new List<Claim>
                    {
                        new Claim("client_type", "headless")
                    },
                    PrefixClientClaims = false,
                },

                new Client
                {
                    ClientName = "Custom Grant Client",
                    Enabled = true,
                    ClientId = "customclient",
                    ClientSecrets = new List<Secret>
                    { 
                        new Secret("secret".Sha256())
                    },

                    Flow = Flows.Custom,

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

                new Client
                {
                    ClientName = "Resource Owner Flow Client",
                    Enabled = true,
                    ClientId = "roclient",
                    ClientSecrets = new List<Secret>
                    { 
                        new Secret("secret".Sha256())
                    },

                    Flow = Flows.ResourceOwner,
                    
                    AllowedScopes = new List<string> 
                    {
                        "openid",
                        "email",
                        "read", 
                        "write",
                        "address",
                        "offline_access"
                    },

                    AllowedCorsOrigins = new List<string>
                    {
                        "http://localhost:13048"
                    },
                    
                    AccessTokenType = AccessTokenType.Jwt,
                    AccessTokenLifetime = 3600,
                    AbsoluteRefreshTokenLifetime = 86400,
                    SlidingRefreshTokenLifetime = 43200,

                    RefreshTokenUsage = TokenUsage.OneTimeOnly,
                    RefreshTokenExpiration = TokenExpiration.Sliding
                },

                new Client
                {
                    ClientName = "UWP Demo Client",
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