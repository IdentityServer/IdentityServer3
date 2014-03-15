using Thinktecture.IdentityServer.Core.Connect;
using Thinktecture.IdentityServer.Core.Connect.Services;
using Thinktecture.IdentityServer.Core.Services;

namespace UnitTests.Plumbing
{
    static class TokenRequestValidatorFactory
    {
        public static TokenRequestValidator Create(
            ICoreSettings settings, 
            ILogger logger, 
            IAuthorizationCodeStore authorizationCodeStore = null,
            IUserService userService = null,
            IAssertionGrantValidator assertionGrantValidator = null,
            ICustomRequestValidator customRequestValidator = null)
        {
            return new TokenRequestValidator(settings, logger, authorizationCodeStore, userService, assertionGrantValidator, customRequestValidator);
        }
    }
}
