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
        public static IdentityServerServiceFactory Create(string issuerUri, string siteName, string publicHostAddress = "")
        {
            var settings = new TestCoreSettings(issuerUri, siteName, publicHostAddress);
            var codeStore = new TestAuthorizationCodeStore();
            var tokenStore = new TestTokenHandleStore();
            var consent = new TestConsentService();
            var logger = new TraceLogger();
            
            var fact = new IdentityServerServiceFactory
            {
                Logger = () => logger,
                UserService = () => new TestUserService(),
                AuthorizationCodeStore = () => codeStore,
                TokenHandleStore = () => tokenStore,
                CoreSettings = () => settings,
                ConsentService = () => consent
            };

            return fact;
        }
    }
}