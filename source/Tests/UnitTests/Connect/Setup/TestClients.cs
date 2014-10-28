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
using System;
using System.Collections.Generic;
using Thinktecture.IdentityServer.Core.Models;

namespace Thinktecture.IdentityServer.Tests.Connect.Setup
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
                        ClientSecret = "secret",
                        Flow = Flows.AuthorizationCode,
                        RequireConsent = false,
                    
                        RedirectUris = new List<Uri>
                        {
                            new Uri("https://server/cb"),
                        },

                        AuthorizationCodeLifetime = 60
                    },
                    new Client
                    {
                        ClientName = "Implicit Client",
                        Enabled = true,
                        ClientId = "implicitclient",
                        ClientSecret = "secret",
                        Flow = Flows.Implicit,
                        RequireConsent = false,
                    
                        RedirectUris = new List<Uri>
                        {
                            new Uri("oob://implicit/cb")
                        },
                    },
                    new Client
                    {
                        ClientName = "Code Client with Scope Restrictions",
                        Enabled = true,
                        ClientId = "codeclient_restricted",
                        ClientSecret = "secret",
                        Flow = Flows.AuthorizationCode,
                        RequireConsent = false,

                        ScopeRestrictions = new List<string>
                        {
                            "openid"
                        },
                    
                        RedirectUris = new List<Uri>
                        {
                            new Uri("https://server/cb"),
                        },
                    },
                    new Client
                    {
                        ClientName = "Client Credentials Client",
                        Enabled = true,
                        ClientId = "client",
                        ClientSecret = "secret",
                        Flow = Flows.ClientCredentials,
                        AccessTokenType = AccessTokenType.Jwt
                    },
                    new Client
                    {
                        ClientName = "Client Credentials Client (restricted)",
                        Enabled = true,
                        ClientId = "client_restricted",
                        ClientSecret = "secret",
                        Flow = Flows.ClientCredentials,

                        ScopeRestrictions = new List<string>
                        {
                            "resource"
                        },       
                    },
                    new Client
                    {
                        ClientName = "Resource Owner Client",
                        Enabled = true,
                        ClientId = "roclient_symmetric",
                        ClientSecret = "V5CQ9HV04yVFOp4WZseN+PUzxtl6sYEcgaJ64IdE7cw=",
                        Flow = Flows.ResourceOwner,
                        IdentityTokenSigningKeyType = SigningKeyTypes.ClientSecret
                    },
                    new Client
                    {
                        ClientName = "Resource Owner Client",
                        Enabled = true,
                        ClientId = "roclient",
                        ClientSecret = "secret",
                        Flow = Flows.ResourceOwner,
                    },
                    new Client
                    {
                        ClientName = "Resource Owner Client",
                        Enabled = true,
                        ClientId = "roclient_absolute_refresh_expiration_one_time_only",
                        ClientSecret = "secret",
                        Flow = Flows.ResourceOwner,

                        RefreshTokenExpiration = TokenExpiration.Absolute,
                        RefreshTokenUsage = TokenUsage.OneTimeOnly,
                        AbsoluteRefreshTokenLifetime = 200
                    },
                    new Client
                    {
                        ClientName = "Resource Owner Client",
                        Enabled = true,
                        ClientId = "roclient_absolute_refresh_expiration_reuse",
                        ClientSecret = "secret",
                        Flow = Flows.ResourceOwner,

                        RefreshTokenExpiration = TokenExpiration.Absolute,
                        RefreshTokenUsage = TokenUsage.ReUse,
                        AbsoluteRefreshTokenLifetime = 200
                    },
                    new Client
                    {
                        ClientName = "Resource Owner Client",
                        Enabled = true,
                        ClientId = "roclient_sliding_refresh_expiration_one_time_only",
                        ClientSecret = "secret",
                        Flow = Flows.ResourceOwner,

                        RefreshTokenExpiration = TokenExpiration.Sliding,
                        RefreshTokenUsage = TokenUsage.OneTimeOnly,
                        AbsoluteRefreshTokenLifetime = 10,
                        SlidingRefreshTokenLifetime = 4
                    },
                    new Client
                    {
                        ClientName = "Resource Owner Client",
                        Enabled = true,
                        ClientId = "roclient_sliding_refresh_expiration_reuse",
                        ClientSecret = "secret",
                        Flow = Flows.ResourceOwner,

                        RefreshTokenExpiration = TokenExpiration.Sliding,
                        RefreshTokenUsage = TokenUsage.ReUse,
                        AbsoluteRefreshTokenLifetime = 200,
                        SlidingRefreshTokenLifetime = 100
                    },
                    new Client
                    {
                        ClientName = "Resource Owner Client (restricted)",
                        Enabled = true,
                        ClientId = "roclient_restricted",
                        ClientSecret = "secret",
                        Flow = Flows.ResourceOwner,

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
                        ClientSecret = "secret",
                        Flow = Flows.ResourceOwner,

                        ScopeRestrictions = new List<string>
                        {
                            "resource",
                            "offline_access"
                        },       
                    },
                    new Client
                    {
                        ClientName = "Assertion Flow Client",
                        Enabled = true,
                        ClientId = "assertionclient",
                        ClientSecret = "secret",
                        Flow = Flows.Custom,
                    },
                    new Client
                    {
                        ClientName = "Disabled Client",
                        Enabled = false,
                        ClientId = "disabled",
                        ClientSecret = "invalid",
                        Flow = Flows.Custom,
                    },
                    new Client
                    {
                        ClientName = "Reference Token Client",

                        Enabled = true,
                        ClientId = "referencetokenclient",
                        ClientSecret = "secret",
                        Flow = Flows.Implicit,

                        AccessTokenType = AccessTokenType.Reference
                    }
            };
        }
    }
}