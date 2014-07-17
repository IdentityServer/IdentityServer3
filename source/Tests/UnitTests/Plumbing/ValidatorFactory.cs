/*
 * Copyright (c) Dominick Baier, Brock Allen.  All rights reserved.
 * see license
 */

using Thinktecture.IdentityServer.Core.Configuration;
using Thinktecture.IdentityServer.Core.Connect;
using Thinktecture.IdentityServer.Core.Services;
using Thinktecture.IdentityServer.Core.Services.InMemory;
using Thinktecture.IdentityServer.Tests.Plumbing;

namespace UnitTests.Plumbing
{
    static class Factory
    {
        public static IClientService CreateClientService()
        {
            return new InMemoryClientService(TestClients.Get());
        }

        public static ClientValidator CreateClientValidator(
            IClientService clients = null)
        {
            if (clients == null)
            {
                clients = new InMemoryClientService(TestClients.Get());
            }

            return new ClientValidator(clients);
        }

        public static TokenRequestValidator CreateTokenValidator(
            IdentityServerOptions options = null,
            IScopeService scopes = null,
            IAuthorizationCodeStore authorizationCodeStore = null,
            IRefreshTokenStore refreshTokens = null,
            IUserService userService = null,
            IAssertionGrantValidator assertionGrantValidator = null,
            ICustomRequestValidator customRequestValidator = null)
        {
            if (options == null)
            {
                options = Thinktecture.IdentityServer.Tests.TestIdentityServerOptions.Create();
            }

            if (scopes == null)
            {
                scopes = new InMemoryScopeService(TestScopes.Get());
            }

            if (userService == null)
            {
                userService = new TestUserService();
            }

            if (customRequestValidator == null)
            {
                customRequestValidator = new DefaultCustomRequestValidator();
            }

            if (assertionGrantValidator == null)
            {
                assertionGrantValidator = new TestAssertionValidator();
            }

            if (refreshTokens == null)
            {
                refreshTokens = new InMemoryRefreshTokenStore();
            }

            return new TokenRequestValidator(options, authorizationCodeStore, refreshTokens, userService, scopes, assertionGrantValidator, customRequestValidator);
        }

        public static AuthorizeRequestValidator CreateAuthorizeValidator(
            IdentityServerOptions options = null,
            IScopeService scopes = null,
            IClientService clients = null,
            IUserService users = null,
            ICustomRequestValidator customValidator = null)
        {
            if (options == null)
            {
                options = Thinktecture.IdentityServer.Tests.TestIdentityServerOptions.Create();
            }

            if (scopes == null)
            {
                scopes = new InMemoryScopeService(TestScopes.Get());
            }

            if (clients == null)
            {
                clients = new InMemoryClientService(TestClients.Get());
            }

            if (customValidator == null)
            {
                customValidator = new DefaultCustomRequestValidator();
            }

            return new AuthorizeRequestValidator(options, scopes, clients, customValidator);
        }
    }
}