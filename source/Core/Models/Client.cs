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
using System.Linq;

namespace Thinktecture.IdentityServer.Core.Models
{
    public class Client
    {
        public bool Enabled { get; set; }

        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public string ClientName { get; set; }
        public string ClientUri { get; set; }
        public Uri LogoUri { get; set; }
        
        public bool RequireConsent { get; set; }
        public bool AllowRememberConsent { get; set; }

        public Flows Flow { get; set; }
        public List<Uri> RedirectUris { get; set; }
        public List<Uri> PostLogoutRedirectUris { get; set; }
        public List<string> ScopeRestrictions { get; set; }
        
        // in seconds
        public int IdentityTokenLifetime { get; set; }
        public int AccessTokenLifetime { get; set; }
        public int AuthorizationCodeLifetime { get; set; }

        // refresh token specific
        public int AbsoluteRefreshTokenLifetime { get; set; }
        public int SlidingRefreshTokenLifetime { get; set; }
        public TokenUsage RefreshTokenUsage { get; set; }
        public TokenExpiration RefreshTokenExpiration { get; set; }
        
        // todo
        //public bool RefreshClaimsOnRefreshToken { get; set; }

        public SigningKeyTypes IdentityTokenSigningKeyType { get; set; }
        public AccessTokenType AccessTokenType { get; set; }

        // login page related
        public bool AllowLocalLogin { get; set; }
        // if empty, all allowed
        public IEnumerable<string> AllowedIdentityProviders { get; set; }

        // not implemented yet
        public bool RequireSignedAuthorizeRequest { get; set; }
        public SubjectTypes SubjectType { get; set; }
        public Uri SectorIdentifierUri { get; set; }
        public ApplicationTypes ApplicationType { get; set; }

        // sensible defaults
        public Client()
        {
            Flow = Flows.Implicit;
            ScopeRestrictions = new List<string>();
            RedirectUris = new List<Uri>();
            PostLogoutRedirectUris = new List<Uri>();
            
            // 5 minutes
            AuthorizationCodeLifetime = 300;

            // one hour
            IdentityTokenLifetime = 3600;
            AccessTokenLifetime = 3600;

            // 30 days
            AbsoluteRefreshTokenLifetime = 2592000;

            // 3 days
            SlidingRefreshTokenLifetime = 259200;

            RefreshTokenUsage = TokenUsage.OneTimeOnly;
            RefreshTokenExpiration = TokenExpiration.Absolute;

            IdentityTokenSigningKeyType = SigningKeyTypes.Default;
            AccessTokenType = AccessTokenType.Jwt;

            AllowedIdentityProviders = Enumerable.Empty<string>();
            AllowLocalLogin = true;
        }
    }
}