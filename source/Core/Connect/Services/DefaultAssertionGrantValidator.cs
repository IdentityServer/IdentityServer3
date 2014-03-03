using System.Security.Claims;

namespace Thinktecture.IdentityServer.Core.Connect.Services
{
    public class DefaultAssertionGrantValidator : IAssertionGrantValidator
    {
        public ClaimsPrincipal Validate(ValidatedTokenRequest request)
        {
            return null;
        }
    }
}