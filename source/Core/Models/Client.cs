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
    /// <summary>
    /// Models an OpenID Connect or OAuth2 client
    /// </summary>
    public class Client
    {
        /// <summary>
        /// Specifies if client is enabled (defaults to false)
        /// </summary>
        public bool Enabled { get; set; }

        /// <summary>
        /// Unique ID of the client
        /// </summary>
        public string ClientId { get; set; }

        /// <summary>
        /// Client secret - only relevant for flows that require a secret
        /// </summary>
        public string ClientSecret { get; set; }

        /// <summary>
        /// Client display name (used for logging and consent screen)
        /// </summary>
        public string ClientName { get; set; }

        /// <summary>
        /// URI to further information about client (used on consent screen)
        /// </summary>
        public string ClientUri { get; set; }
        
        /// <summary>
        /// URI to client logo (used on consent screen)
        /// </summary>
        public Uri LogoUri { get; set; }
        
        /// <summary>
        /// Specifies whether a consent screen is required (defaults to false)
        /// </summary>
        public bool RequireConsent { get; set; }

        /// <summary>
        /// Specifies whether user can choose to store consent decisions (defaults to false)
        /// </summary>
        public bool AllowRememberConsent { get; set; }

        /// <summary>
        /// Specifies allowed flow for client (either AuthorizationCode, Implicit, Hybrid, ResourceOwner, ClientCredentials or Custom). Defaults to Implicit.
        /// </summary>
        public Flows Flow { get; set; }

        /// <summary>
        /// Specifies allowed URIs to return tokens or authorization codes to
        /// </summary>
        public List<Uri> RedirectUris { get; set; }

        /// <summary>
        /// Specifies allowed URIs to redirect to after logout
        /// </summary>
        public List<Uri> PostLogoutRedirectUris { get; set; }
        
        /// <summary>
        /// Specifies the scopes that the client is allowed to request. If empty, the client can request all scopes (defaults to empty)
        /// </summary>
        public List<string> ScopeRestrictions { get; set; }
        
        /// <summary>
        /// Lifetime of identity token in seconds (defaults to 300 seconds / 5 minutes)
        /// </summary>
        public int IdentityTokenLifetime { get; set; }

        /// <summary>
        /// Lifetime of access token in seconds (defaults to 3600 seconds / 1 hour)
        /// </summary>
        public int AccessTokenLifetime { get; set; }

        /// <summary>
        /// Lifetime of authorization code in seconds (defaults to 300 seconds / 5 minutes)
        /// </summary>
        public int AuthorizationCodeLifetime { get; set; }

        /// <summary>
        /// Maximum lifetime of a refresh token in seconds. Defaults to 2592000 seconds / 30 days
        /// </summary>
        public int AbsoluteRefreshTokenLifetime { get; set; }
        
        /// <summary>
        /// Sliding lifetime of a refresh token in seconds. Defaults to 1296000 seconds / 15 days
        /// </summary>
        public int SlidingRefreshTokenLifetime { get; set; }
        
        /// <summary>
        /// Absolute: the refresh token will expire on a fixed point in time (specified by the AbsoluteRefreshTokenLifetime)
        /// Sliding: when refreshing the token, the lifetime of the refresh token will be renewed (by the amount specified in SlidingRefreshTokenLifetime). The lifetime will not exceed 
        /// </summary>
        public TokenUsage RefreshTokenUsage { get; set; }

        /// <summary>
        /// ReUse: the refresh token handle will stay the same when refreshing tokens
        /// OneTime: the refresh token handle will be updated when refreshing tokens
        /// </summary>
        public TokenExpiration RefreshTokenExpiration { get; set; }
        
        /// <summary>
        /// Specifies the key material used to sign the identity token. Default for the primary X.509 certificate, ClientSecret for using the client secret as a symmetric key (must be 256 bits in length). Defaults to Default.
        /// </summary>
        public SigningKeyTypes IdentityTokenSigningKeyType { get; set; }
        
        /// <summary>
        /// Specifies whether the access token is a reference token or a self contained JWT token (defaults to Jwt).
        /// </summary>
        public AccessTokenType AccessTokenType { get; set; }

        /// <summary>
        /// Specifies if this client can use local accounts, or external IdPs only
        /// </summary>
        public bool AllowLocalLogin { get; set; }
        
        /// <summary>
        /// Specifies which external IdPs can be used with this client (if list is empty all IdPs are allowed). Defaults to empty.
        /// </summary>
        public IEnumerable<string> IdentityProviderRestrictions { get; set; }

        // not implemented yet

        //public bool RefreshClaimsOnRefreshToken { get; set; }
        //public bool RequireSignedAuthorizeRequest { get; set; }
        //public SubjectTypes SubjectType { get; set; }
        //public Uri SectorIdentifierUri { get; set; }
        //public ApplicationTypes ApplicationType { get; set; }

        /// <summary>
        /// Creates a Client with default values
        /// </summary>
        public Client()
        {
            Flow = Flows.Implicit;
            ScopeRestrictions = new List<string>();
            RedirectUris = new List<Uri>();
            PostLogoutRedirectUris = new List<Uri>();
            
            // 5 minutes
            AuthorizationCodeLifetime = 300;
            IdentityTokenLifetime = 300;

            // one hour
            AccessTokenLifetime = 3600;

            // 30 days
            AbsoluteRefreshTokenLifetime = 2592000;

            // 15 days
            SlidingRefreshTokenLifetime = 1296000;

            RefreshTokenUsage = TokenUsage.OneTimeOnly;
            RefreshTokenExpiration = TokenExpiration.Absolute;

            IdentityTokenSigningKeyType = SigningKeyTypes.Default;
            AccessTokenType = AccessTokenType.Jwt;

            IdentityProviderRestrictions = Enumerable.Empty<string>();
            AllowLocalLogin = true;
        }
    }
}