/*
 * Copyright (c) Dominick Baier, Brock Allen.  All rights reserved.
 * see license
 */

using System;
using Thinktecture.IdentityServer.Core.Connect.Services;
using Thinktecture.IdentityServer.Core.Services;

namespace Thinktecture.IdentityServer.Core.Configuration
{
    public class IdentityServerServiceFactory
    {
        // mandatory (external)
        public Func<IUserService> UserService { get; set; }
        public Func<IScopeService> ScopeService { get; set; }
        public Func<IClientService> ClientService { get; set; }
        public Func<CoreSettings> CoreSettings { get; set; }
        
        // mandatory (internal)
        public Func<IAuthorizationCodeStore> AuthorizationCodeStore { get; set; }
        public Func<ITokenHandleStore> TokenHandleStore { get; set; }
        public Func<IConsentService> ConsentService { get; set; }
        
        // optional
        public Func<IAssertionGrantValidator> AssertionGrantValidator { get; set; }
        public Func<ICustomRequestValidator> CustomRequestValidator { get; set; }
        public Func<IClaimsProvider> ClaimsProvider { get; set; }
        public Func<ITokenService> TokenService { get; set; }
        public Func<IExternalClaimsFilter> ExternalClaimsFilter { get; set; }
        public Func<ICustomTokenValidator> CustomTokenValidator { get; set; }

        public void Validate()
        {
            if (UserService == null) throw new InvalidOperationException("UserService not configured");
            if (CoreSettings == null) throw new InvalidOperationException("CoreSettings not configured");
            if (ScopeService == null) throw new InvalidOperationException("ScopeService not configured.");
            if (ClientService == null) throw new InvalidOperationException("ClientService not configured.");

            if (AuthorizationCodeStore == null) throw new InvalidOperationException("AuthorizationCodeStore not configured");
            if (TokenHandleStore == null) throw new InvalidOperationException("TokenHandleStore not configured");
            if (ConsentService == null) throw new InvalidOperationException("ConsentService not configured");
        }
    }
}