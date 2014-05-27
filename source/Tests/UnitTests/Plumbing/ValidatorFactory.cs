/*
 * Copyright (c) Dominick Baier, Brock Allen.  All rights reserved.
 * see license
 */
using Thinktecture.IdentityServer.Core.Connect;
using Thinktecture.IdentityServer.Core.Connect.Services;
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
            ILogger logger = null,
            IClientService clients = null)
        {
            if (logger == null)
            {
                logger = new DebugLogger();
            }

            if (clients == null)
            {
                clients = new InMemoryClientService(TestClients.Get());
            }

            return new ClientValidator(clients, logger);
        }

        public static TokenRequestValidator CreateTokenValidator(
            CoreSettings settings = null,
            ILogger logger = null,
            IScopeService scopes = null,
            IAuthorizationCodeStore authorizationCodeStore = null,
            IUserService userService = null,
            IAssertionGrantValidator assertionGrantValidator = null,
            ICustomRequestValidator customRequestValidator = null)
        {
            if (settings == null)
            {
                settings = new TestSettings();
            }

            if (scopes == null)
            {
                scopes = new InMemoryScopeService(TestScopes.Get());
            }

            if (logger == null)
            {
                logger = new DebugLogger();
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

            return new TokenRequestValidator(settings, logger, authorizationCodeStore, userService, scopes, assertionGrantValidator, customRequestValidator);
        }

        public static AuthorizeRequestValidator CreateAuthorizeValidator(
            CoreSettings settings = null,
            IScopeService scopes = null,
            IClientService clients = null,
            ILogger logger = null,
            IUserService users = null,
            ICustomRequestValidator customValidator = null)
        {
            if (settings == null)
            {
                settings = new TestSettings();
            }

            if (scopes == null)
            {
                scopes = new InMemoryScopeService(TestScopes.Get());
            }

            if (clients == null)
            {
                clients = new InMemoryClientService(TestClients.Get());
            }

            if (logger == null)
            {
                logger = new DebugLogger();
            }

            if (customValidator == null)
            {
                customValidator = new DefaultCustomRequestValidator();
            }

            if (users == null)
            {
                users = new TestUserService();
            }

            return new AuthorizeRequestValidator(settings, scopes, clients, logger, users, customValidator);
        }
    }
}