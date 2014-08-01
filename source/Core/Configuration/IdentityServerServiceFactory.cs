/*
 * Copyright (c) Dominick Baier, Brock Allen.  All rights reserved.
 * see license
 */

using System;
using System.Collections.Generic;
using Thinktecture.IdentityServer.Core.Logging;
using Thinktecture.IdentityServer.Core.Services;

namespace Thinktecture.IdentityServer.Core.Configuration
{
    public class IdentityServerServiceFactory
    {
        static readonly ILog Logger = LogProvider.GetCurrentClassLogger();

        // keep list of any additional dependencies the 
        // hosting application might need. these will be
        // added to the DI container
        List<Registration> _registrations = new List<Registration>();
        public IEnumerable<Registration> Registrations
        {
            get { return _registrations; }
        }

        public void Register<T>(Registration<T> r)
            where T : class
        {
            _registrations.Add(r);
        }

        // mandatory (external)
        public Registration<IUserService> UserService { get; set; }
        public Registration<IScopeStore> ScopeStore { get; set; }
        public Registration<IClientStore> ClientStore { get; set; }
        
        // mandatory (for authorization code, reference & refresh tokens and consent)
        // but with default in memory implementation
        public Registration<IAuthorizationCodeStore> AuthorizationCodeStore { get; set; }
        public Registration<ITokenHandleStore> TokenHandleStore { get; set; }
        public Registration<IConsentStore> ConsentStore { get; set; }
        public Registration<IRefreshTokenStore> RefreshTokenStore { get; set; }
        public Registration<IViewService> ViewService { get; set; }
        
        // optional
        public Registration<IConsentService> ConsentService { get; set; }
        public Registration<IAssertionGrantValidator> AssertionGrantValidator { get; set; }
        public Registration<ICustomRequestValidator> CustomRequestValidator { get; set; }
        public Registration<IClaimsProvider> ClaimsProvider { get; set; }
        public Registration<ITokenService> TokenService { get; set; }
        public Registration<IRefreshTokenService> RefreshTokenService { get; set; }
        public Registration<ITokenSigningService> TokenSigningService { get; set; }
        public Registration<IExternalClaimsFilter> ExternalClaimsFilter { get; set; }
        public Registration<ICustomTokenValidator> CustomTokenValidator { get; set; }

        public void Validate()
        {
            if (UserService == null) LogAndStop("UserService not configured");
            if (ScopeStore == null) LogAndStop("ScopeStore not configured.");
            if (ClientStore == null) LogAndStop("ClientStore not configured.");

            if (AuthorizationCodeStore == null) Logger.Warn("AuthorizationCodeStore not configured - falling back to InMemory");
            if (TokenHandleStore == null) Logger.Warn("TokenHandleStore not configured - falling back to InMemory");
            if (ConsentService == null) Logger.Warn("ConsentService not configured - falling back to InMemory");
            if (RefreshTokenStore == null) Logger.Warn("RefreshTokenStore not configured - falling back to InMemory");
            if (ViewService == null) Logger.Info("ViewService not configured - falling back to EmbeddedAssets");
        }

        private void LogAndStop(string message)
        {
            Logger.Error(message);
            throw new InvalidOperationException(message);
        }
    }
}