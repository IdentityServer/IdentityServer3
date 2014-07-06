/*
 * Copyright (c) Dominick Baier, Brock Allen.  All rights reserved.
 * see license
 */

using System;
using System.Linq;
using System.Collections.Generic;
using Thinktecture.IdentityServer.Core.Connect.Services;
using Thinktecture.IdentityServer.Core.Logging;
using Thinktecture.IdentityServer.Core.Services;

namespace Thinktecture.IdentityServer.Core.Configuration
{
    public class IdentityServerServiceFactory
    {
        static ILog Logger = LogProvider.GetCurrentClassLogger();

        // keep list of any additional dependencies the 
        // hosting application might need. these will be
        // added to the DI container
        List<Registration> registrations = new List<Registration>();
        public IEnumerable<Registration> Registrations
        {
            get { return registrations; }
        }

        public void Register<T>(Registration<T> r)
            where T : class
        {
            registrations.Add(r);
        }

        // mandatory (external)
        public Registration<IUserService> UserService { get; set; }
        public Registration<IScopeService> ScopeService { get; set; }
        public Registration<IClientService> ClientService { get; set; }
        public Registration<CoreSettings> CoreSettings { get; set; }
        
        // mandatory (for authorization code, reference tokens and consent)
        // but with default in memory implementation
        public Registration<IAuthorizationCodeStore> AuthorizationCodeStore { get; set; }
        public Registration<ITokenHandleStore> TokenHandleStore { get; set; }
        public Registration<IConsentService> ConsentService { get; set; }
        
        // optional
        public Registration<IAssertionGrantValidator> AssertionGrantValidator { get; set; }
        public Registration<ICustomRequestValidator> CustomRequestValidator { get; set; }
        public Registration<IClaimsProvider> ClaimsProvider { get; set; }
        public Registration<ITokenService> TokenService { get; set; }
        public Registration<IExternalClaimsFilter> ExternalClaimsFilter { get; set; }
        public Registration<ICustomTokenValidator> CustomTokenValidator { get; set; }

        public void Validate()
        {
            if (UserService == null) throw new InvalidOperationException("UserService not configured");
            if (CoreSettings == null) throw new InvalidOperationException("CoreSettings not configured");
            if (ScopeService == null) throw new InvalidOperationException("ScopeService not configured.");
            if (ClientService == null) throw new InvalidOperationException("ClientService not configured.");

            if (AuthorizationCodeStore == null) Logger.Warn("AuthorizationCodeStore not configured - falling back to InMemory");
            if (TokenHandleStore == null) Logger.Warn("TokenHandleStore not configured - falling back to InMemory");
            if (ConsentService == null) Logger.Warn("ConsentService not configured - falling back to InMemory");
        }
    }
}