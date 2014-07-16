/*
 * Copyright (c) Dominick Baier, Brock Allen.  All rights reserved.
 * see license
 */

using System;
using System.Collections.Generic;

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
        }
    }
}