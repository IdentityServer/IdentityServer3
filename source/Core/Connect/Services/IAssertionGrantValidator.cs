using System.Security.Claims;

namespace Thinktecture.IdentityServer.Core.Connect.Services
{
    public interface IAssertionGrantValidator
    {
        ClaimsPrincipal Validate(ValidatedTokenRequest request);
    }
}
