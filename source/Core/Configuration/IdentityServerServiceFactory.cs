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
        // mandatory
        public Func<ILogger> Logger { get; set; }
        public Func<IUserService> UserService { get; set; }
        public Func<IScopeService> ScopeService { get; set; }
        public Func<IClientService> ClientService { get; set; }
        public Func<ICoreSettings> CoreSettings { get; set; }
        
        // internal storage
        public Func<IAuthorizationCodeStore> AuthorizationCodeStore { get; set; }
        public Func<ITokenHandleStore> TokenHandleStore { get; set; }
        public Func<IConsentService> ConsentService { get; set; }
        
        // optional
        public Func<IAssertionGrantValidator> AssertionGrantValidator { get; set; }
        public Func<ICustomRequestValidator> CustomRequestValidator { get; set; }
        public Func<IClaimsProvider> ClaimsProvider { get; set; }
        public Func<ITokenService> TokenService { get; set; }
        public Func<IExternalClaimsFilter> ExternalClaimsFilter { get; set; }

        internal void Validate()
        {
            if (Logger == null) throw new InvalidOperationException("Logger not configured");
            if (UserService == null) throw new InvalidOperationException("UserService not configured");
            if (CoreSettings == null) throw new InvalidOperationException("CoreSettings not configured");
            if (AuthorizationCodeStore == null) throw new InvalidOperationException("AuthorizationCodeStore not configured");
            if (TokenHandleStore == null) throw new InvalidOperationException("TokenHandleStore not configured");
            if (ConsentService == null) throw new InvalidOperationException("ConsentService not configured");
        }
    }
}
