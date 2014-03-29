/*
 * Copyright (c) Dominick Baier, Brock Allen.  All rights reserved.
 * see license
 */
using Thinktecture.IdentityServer.Core.Configuration;
using Thinktecture.IdentityServer.Core.Services;

namespace Thinktecture.IdentityServer.TestServices
{
    public class TestOptionsFactory
    {
        public static IdentityServerServiceFactory Create()
        {
            var codeStore = new TestAuthorizationCodeStore();
            var tokenStore = new TestTokenHandleStore();
            var core = new TestCoreSettings();
            var consent = new TestConsentService();
            var logger = new DebugLogger();
            
            var fact = new IdentityServerServiceFactory
            {
                Logger = () => logger,
                UserService = () => new TestUserService(),
                AuthorizationCodeStore = () => codeStore,
                TokenHandleStore = () => tokenStore,
                CoreSettings = () => core,
                ConsentService = () => consent
            };

            return fact;
        }
    }
}