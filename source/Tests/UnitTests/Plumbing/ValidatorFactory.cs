/*
 * Copyright (c) Dominick Baier, Brock Allen.  All rights reserved.
 * see license
 */
using Thinktecture.IdentityServer.Core.Connect;
using Thinktecture.IdentityServer.Core.Connect.Services;
using Thinktecture.IdentityServer.Core.Services;

namespace UnitTests.Plumbing
{
    static class ValidatorFactory
    {
        public static TokenRequestValidator CreateTokenValidator(
            ICoreSettings settings,
            ILogger logger,
            IAuthorizationCodeStore authorizationCodeStore = null,
            IUserService userService = null,
            IAssertionGrantValidator assertionGrantValidator = null,
            ICustomRequestValidator customRequestValidator = null)
        {
            return new TokenRequestValidator(settings, logger, authorizationCodeStore, userService, assertionGrantValidator, customRequestValidator);
        }

        public static AuthorizeRequestValidator CreateAuthorizeValidator(
            ICoreSettings settings = null,
            ILogger logger = null,
            IUserService users = null,
            ICustomRequestValidator customValidator = null)
        {
            if (settings == null)
            {
                settings = new TestSettings();
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


            return new AuthorizeRequestValidator(settings, logger, users, customValidator);
        }
    }
}