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

using System.Collections.Generic;
using Thinktecture.IdentityServer.Core.Extensions;
using Thinktecture.IdentityServer.Core.Models;

namespace Thinktecture.IdentityServer.Tests.Validation.Setup
{
    class TestClients
    {
        public static IEnumerable<Client> Get()
        {
            return new List<Client>
            {
                new Client
                {
                        ClientName = "Code Client",
                        Enabled = true,
                        ClientId = "codeclient",
                        ClientSecrets = new List<ClientSecret>
                        { 
                            new ClientSecret("secret".Sha256())
                        },

                        Flow = Flows.AUTHORIZATION_CODE,
                        
                        RequireConsent = false,
                    
                        RedirectUris = new List<string>
                        {
                            "https://server/cb",
                        },

                        AuthorizationCodeLifetime = 60
                    },

                    new Client
                {
                        ClientName = "Hybrid Client",
                        Enabled = true,
                        ClientId = "hybridclient",
                        ClientSecrets = new List<ClientSecret>
                        { 
                            new ClientSecret("secret".Sha256())
                        },

                        Flow = Flows.AUTHORIZATION_CODE,
                        
                        RequireConsent = false,
                    
                        RedirectUris = new List<string>
                        {
                            "https://server/cb",
                        },

                        AuthorizationCodeLifetime = 60
                    },
                    new Client
                    {
                        ClientName = "Implicit Client",
                        Enabled = true,
                        ClientId = "implicitclient",
                        ClientSecrets = new List<ClientSecret>
                        { 
                            new ClientSecret("secret".Sha256())
                        },

                        Flow = Flows.IMPLICIT,
                        RequireConsent = false,
                    
                        RedirectUris = new List<string>
                        {
                            "oob://implicit/cb"
                        },
                    },
                    new Client
                    {
                        ClientName = "Implicit and Client Credentials Client",
                        Enabled = true,
                        ClientId = "implicit_and_client_creds_client",
                        ClientSecrets = new List<ClientSecret>
                        { 
                            new ClientSecret("secret".Sha256())
                        },

                        Flow = Flows.IMPLICIT,
                        AllowClientCredentialsOnly = true,
                        RequireConsent = false,
                    
                        RedirectUris = new List<string>
                        {
                            "oob://implicit/cb"
                        },
                    },
                    new Client
                    {
                        ClientName = "Code Client with Scope Restrictions",
                        Enabled = true,
                        ClientId = "codeclient_restricted",
                        ClientSecrets = new List<ClientSecret>
                        { 
                            new ClientSecret("secret".Sha256())
                        },

                        Flow = Flows.AUTHORIZATION_CODE,
                        RequireConsent = false,

                        ScopeRestrictions = new List<string>
                        {
                            "openid"
                        },
                    
                        RedirectUris = new List<string>
                        {
                            "https://server/cb",
                        },
                    },
                    new Client
                    {
                        ClientName = "Client Credentials Client",
                        Enabled = true,
                        ClientId = "client",
                        ClientSecrets = new List<ClientSecret>
                        { 
                            new ClientSecret("secret".Sha256())
                        },

                        Flow = Flows.CLIENT_CREDENTIALS,
                        AccessTokenType = AccessTokenType.JWT
                    },
                    new Client
                    {
                        ClientName = "Client Credentials Client (restricted)",
                        Enabled = true,
                        ClientId = "client_restricted",
                        ClientSecrets = new List<ClientSecret>
                        { 
                            new ClientSecret("secret".Sha256())
                        },

                        Flow = Flows.CLIENT_CREDENTIALS,

                        ScopeRestrictions = new List<string>
                        {
                            "resource"
                        },       
                    },
                    new Client
                    {
                        ClientName = "Resource Owner Client",
                        Enabled = true,
                        ClientId = "roclient",
                        ClientSecrets = new List<ClientSecret>
                        { 
                            new ClientSecret("secret".Sha256())
                        },

                        Flow = Flows.RESOURCE_OWNER,
                    },
                    new Client
                    {
                        ClientName = "Resource Owner Client",
                        Enabled = true,
                        ClientId = "roclient_absolute_refresh_expiration_one_time_only",
                        ClientSecrets = new List<ClientSecret>
                        { 
                            new ClientSecret("secret".Sha256())
                        },

                        Flow = Flows.RESOURCE_OWNER,

                        RefreshTokenExpiration = TokenExpiration.ABSOLUTE,
                        RefreshTokenUsage = TokenUsage.ONE_TIME_ONLY,
                        AbsoluteRefreshTokenLifetime = 200
                    },
                    new Client
                    {
                        ClientName = "Resource Owner Client",
                        Enabled = true,
                        ClientId = "roclient_absolute_refresh_expiration_reuse",
                        ClientSecrets = new List<ClientSecret>
                        { 
                            new ClientSecret("secret".Sha256())
                        },

                        Flow = Flows.RESOURCE_OWNER,

                        RefreshTokenExpiration = TokenExpiration.ABSOLUTE,
                        RefreshTokenUsage = TokenUsage.RE_USE,
                        AbsoluteRefreshTokenLifetime = 200
                    },
                    new Client
                    {
                        ClientName = "Resource Owner Client",
                        Enabled = true,
                        ClientId = "roclient_sliding_refresh_expiration_one_time_only",
                        ClientSecrets = new List<ClientSecret>
                        { 
                            new ClientSecret("secret".Sha256())
                        },

                        Flow = Flows.RESOURCE_OWNER,

                        RefreshTokenExpiration = TokenExpiration.SLIDING,
                        RefreshTokenUsage = TokenUsage.ONE_TIME_ONLY,
                        AbsoluteRefreshTokenLifetime = 10,
                        SlidingRefreshTokenLifetime = 4
                    },
                    new Client
                    {
                        ClientName = "Resource Owner Client",
                        Enabled = true,
                        ClientId = "roclient_sliding_refresh_expiration_reuse",
                        ClientSecrets = new List<ClientSecret>
                        { 
                            new ClientSecret("secret".Sha256())
                        },

                        Flow = Flows.RESOURCE_OWNER,

                        RefreshTokenExpiration = TokenExpiration.SLIDING,
                        RefreshTokenUsage = TokenUsage.RE_USE,
                        AbsoluteRefreshTokenLifetime = 200,
                        SlidingRefreshTokenLifetime = 100
                    },
                    new Client
                    {
                        ClientName = "Resource Owner Client (restricted)",
                        Enabled = true,
                        ClientId = "roclient_restricted",
                        ClientSecrets = new List<ClientSecret>
                        { 
                            new ClientSecret("secret".Sha256())
                        },

                        Flow = Flows.RESOURCE_OWNER,

                        ScopeRestrictions = new List<string>
                        {
                            "resource"
                        },       
                    },
                    new Client
                    {
                        ClientName = "Resource Owner Client (restricted with refresh)",
                        Enabled = true,
                        ClientId = "roclient_restricted_refresh",
                        ClientSecrets = new List<ClientSecret>
                        { 
                            new ClientSecret("secret".Sha256())
                        },

                        Flow = Flows.RESOURCE_OWNER,

                        ScopeRestrictions = new List<string>
                        {
                            "resource",
                            "offline_access"
                        },       
                    },
                    new Client
                    {
                        ClientName = "Custom Grant Client",
                        Enabled = true,
                        ClientId = "customgrantclient",
                        ClientSecrets = new List<ClientSecret>
                        { 
                            new ClientSecret("secret".Sha256())
                        },

                        Flow = Flows.CUSTOM,
                        CustomGrantTypeRestrictions = new List<string>
                        {
                            "custom_grant"
                        }

                    },
                    new Client
                    {
                        ClientName = "Disabled Client",
                        Enabled = false,
                        ClientId = "disabled",
                        ClientSecrets = new List<ClientSecret>
                        { 
                            new ClientSecret("invalid".Sha256())
                        },

                        Flow = Flows.CUSTOM,
                    },
                    new Client
                    {
                        ClientName = "Reference Token Client",

                        Enabled = true,
                        ClientId = "referencetokenclient",
                        ClientSecrets = new List<ClientSecret>
                        { 
                            new ClientSecret("secret".Sha256())
                        },

                        Flow = Flows.IMPLICIT,

                        AccessTokenType = AccessTokenType.REFERENCE
                    }
            };
        }
    }
}