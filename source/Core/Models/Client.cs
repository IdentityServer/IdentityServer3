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
using System.Security.Claims;

namespace IdentityServer3.Core.Models
{
    /// <summary>
    /// Models an OpenID Connect or OAuth2 client
    /// </summary>
    public class Client
    {
        /// <summary>
        /// Specifies if client is enabled (defaults to true)
        /// </summary>
        public bool Enabled { get; set; }

        /// <summary>
        /// Unique ID of the client
        /// </summary>
        public string ClientId { get; set; }

        /// <summary>
        /// Client secrets - only relevant for flows that require a secret
        /// </summary>
        public List<Secret> ClientSecrets { get; set; }

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
        public string LogoUri { get; set; }
        
        /// <summary>
        /// Specifies whether a consent screen is required (defaults to true)
        /// </summary>
        public bool RequireConsent { get; set; }

        /// <summary>
        /// Specifies whether user can choose to store consent decisions (defaults to true)
        /// </summary>
        public bool AllowRememberConsent { get; set; }

        /// <summary>
        /// Specifies allowed flow for client (either AuthorizationCode, Implicit, Hybrid, ResourceOwner, ClientCredentials or Custom). Defaults to Implicit.
        /// </summary>
        public Flows Flow { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this client is allowed to request token using client credentials only.
        /// This is e.g. useful when you want a client to be able to use both a user-centric flow like implicit and additionally client credentials flow
        /// </summary>
        /// <value>
        /// <c>true</c> if client credentials flow is allowed; otherwise, <c>false</c>.
        /// </value>
        public bool AllowClientCredentialsOnly { get; set; }

        /// <summary>
        /// Specifies allowed URIs to return tokens or authorization codes to
        /// </summary>
        public List<string> RedirectUris { get; set; }

        /// <summary>
        /// Specifies allowed URIs to redirect to after logout
        /// </summary>
        public List<string> PostLogoutRedirectUris { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the client has access to all scopes. Defaults to false.
        /// You can set the allowed scopes via the AllowedScopes list.
        /// </summary>
        /// <value>
        /// <c>true</c> if client has access to all scopes; otherwise, <c>false</c>.
        /// </value>
        public bool AllowAccessToAllScopes { get; set; }

        /// <summary>
        /// Specifies the scopes that the client is allowed to request. If empty, the client can't access any scope
        /// </summary>
        public List<string> AllowedScopes { get; set; }
        
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
        /// ReUse: the refresh token handle will stay the same when refreshing tokens
        /// OneTime: the refresh token handle will be updated when refreshing tokens
        /// </summary>
        public TokenUsage RefreshTokenUsage { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the access token (and its claims) should be updated on a refresh token request.
        /// </summary>
        /// <value>
        /// <c>true</c> if the token should be updated; otherwise, <c>false</c>.
        /// </value>
        public bool UpdateAccessTokenClaimsOnRefresh { get; set; }

        /// <summary>
        /// Absolute: the refresh token will expire on a fixed point in time (specified by the AbsoluteRefreshTokenLifetime)
        /// Sliding: when refreshing the token, the lifetime of the refresh token will be renewed (by the amount specified in SlidingRefreshTokenLifetime). The lifetime will not exceed AbsoluteRefreshTokenLifetime.
        /// </summary>        
        public TokenExpiration RefreshTokenExpiration { get; set; }
        
        /// <summary>
        /// Specifies whether the access token is a reference token or a self contained JWT token (defaults to Jwt).
        /// </summary>
        public AccessTokenType AccessTokenType { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the local login is allowed for this client. Defaults to <c>true</c>.
        /// </summary>
        /// <value>
        ///   <c>true</c> if local logins are enabled; otherwise, <c>false</c>.
        /// </value>
        public bool EnableLocalLogin { get; set; }
        
        /// <summary>
        /// Specifies which external IdPs can be used with this client (if list is empty all IdPs are allowed). Defaults to empty.
        /// </summary>
        public List<string> IdentityProviderRestrictions { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether JWT access tokens should include an identifier
        /// </summary>
        /// <value>
        /// <c>true</c> to add an id; otherwise, <c>false</c>.
        /// </value>
        public bool IncludeJwtId { get; set; }

        /// <summary>
        /// Allows settings claims for the client (will be included in the access token).
        /// </summary>
        /// <value>
        /// The claims.
        /// </value>
        public List<Claim> Claims { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether client claims should be always included in the access tokens - or only for client credentials flow.
        /// </summary>
        /// <value>
        /// <c>true</c> if claims should always be sent; otherwise, <c>false</c>.
        /// </value>
        public bool AlwaysSendClientClaims { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether all client claims should be prefixed.
        /// </summary>
        /// <value>
        /// <c>true</c> if client claims should be prefixed; otherwise, <c>false</c>.
        /// </value>
        public bool PrefixClientClaims { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the client has access to all custom grant types. Defaults to false.
        /// You can set the allowed custom grant types via the AllowedCustomGrantTypes list.
        /// </summary>
        /// <value>
        /// <c>true</c> if client has access to all custom grant types; otherwise, <c>false</c>.
        /// </value>
        public bool AllowAccessToAllCustomGrantTypes { get; set; }
        
        /// <summary>
        /// Gets or sets a list of allowed custom grant types when Flow is set to Custom.
        /// </summary>
        /// <value>
        /// The allowed custom grant types.
        /// </value>
        public List<string> AllowedCustomGrantTypes { get; set; }

        /// <summary>
        /// Gets or sets the allowed CORS origins for JavaScript clients.
        /// </summary>
        /// <value>
        /// The allowed CORS origins.
        /// </value>
        public List<string> AllowedCorsOrigins { get; set; }

        /// <summary>
        /// Creates a Client with default values
        /// </summary>
        public Client()
        {
            Flow = Flows.Implicit;
            
            ClientSecrets = new List<Secret>();
            AllowedScopes = new List<string>();
            RedirectUris = new List<string>();
            PostLogoutRedirectUris = new List<string>();
            IdentityProviderRestrictions = new List<string>();
            AllowedCustomGrantTypes = new List<string>();
            AllowedCorsOrigins = new List<string>();

            Enabled = true;
            EnableLocalLogin = true;
            AllowAccessToAllScopes = false;
            AllowAccessToAllCustomGrantTypes = false;

            // client claims settings
            Claims = new List<Claim>();
            AlwaysSendClientClaims = false;
            PrefixClientClaims = true;
            
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

            AccessTokenType = AccessTokenType.Jwt;
            
            RequireConsent = true;
            AllowRememberConsent = true;
        }
    }
}
