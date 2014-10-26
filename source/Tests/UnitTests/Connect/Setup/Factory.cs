/*
 * Copyright 2014 Dominick Baier, Brock Allen
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

using Microsoft.Owin;
using System.Collections.Generic;
using Thinktecture.IdentityServer.Core.Configuration;
using Thinktecture.IdentityServer.Core.Services;
using Thinktecture.IdentityServer.Core.Services.Default;
using Thinktecture.IdentityServer.Core.Services.InMemory;
using Thinktecture.IdentityServer.Core.Validation;

namespace Thinktecture.IdentityServer.Tests.Connect.Setup
{
    static class Factory
    {
        public static IClientStore CreateClientStore()
        {
            return new InMemoryClientStore(TestClients.Get());
        }

        public static ClientValidator CreateClientValidator(
            IClientStore clients = null)
        {
            if (clients == null)
            {
                clients = new InMemoryClientStore(TestClients.Get());
            }

            return new ClientValidator(clients);
        }

        public static TokenRequestValidator CreateTokenRequestValidator(
            IdentityServerOptions options = null,
            IScopeStore scopes = null,
            IAuthorizationCodeStore authorizationCodeStore = null,
            IRefreshTokenStore refreshTokens = null,
            IUserService userService = null,
            ICustomGrantValidator customGrantValidator = null,
            ICustomRequestValidator customRequestValidator = null,
            IDictionary<string, object> environment = null)
        {
            if (options == null)
            {
                options = Thinktecture.IdentityServer.Tests.TestIdentityServerOptions.Create();
            }

            if (scopes == null)
            {
                scopes = new InMemoryScopeStore(TestScopes.Get());
            }

            if (userService == null)
            {
                userService = new TestUserService();
            }

            if (customRequestValidator == null)
            {
                customRequestValidator = new DefaultCustomRequestValidator();
            }

            if (customGrantValidator == null)
            {
                customGrantValidator = new TestGrantValidator();
            }

            if (refreshTokens == null)
            {
                refreshTokens = new InMemoryRefreshTokenStore();
            }

            IOwinContext context;
            if (environment == null)
            {
                context = new OwinContext(new Dictionary<string, object>());
            }
            else
            {
                context = new OwinContext(environment);
            }


            return new TokenRequestValidator(options, authorizationCodeStore, refreshTokens, userService, scopes, customGrantValidator, customRequestValidator, context);
        }

        public static AuthorizeRequestValidator CreateAuthorizeRequestValidator(
            IdentityServerOptions options = null,
            IScopeStore scopes = null,
            IClientStore clients = null,
            IUserService users = null,
            ICustomRequestValidator customValidator = null,
            IDictionary<string, object> environment = null)
        {
            if (options == null)
            {
                options = Thinktecture.IdentityServer.Tests.TestIdentityServerOptions.Create();
            }

            if (scopes == null)
            {
                scopes = new InMemoryScopeStore(TestScopes.Get());
            }

            if (clients == null)
            {
                clients = new InMemoryClientStore(TestClients.Get());
            }

            if (customValidator == null)
            {
                customValidator = new DefaultCustomRequestValidator();
            }

            IOwinContext context;
            if (environment == null)
            {
                context = new OwinContext(new Dictionary<string, object>());
            }
            else
            {
                context = new OwinContext(environment);
            }

            return new AuthorizeRequestValidator(options, scopes, clients, customValidator, context);
        }

        public static TokenValidator CreateTokenValidator(ITokenHandleStore tokenStore = null)
        {
            var users = new TestUserService();
            var clients = CreateClientStore();

            var validator = new TokenValidator(
                options: TestIdentityServerOptions.Create(),
                users: users,
                clients: clients,
                tokenHandles: tokenStore,
                customValidator: new DefaultCustomTokenValidator(
                    users: users,
                    clients: clients));

            return validator;
        }
    }
}