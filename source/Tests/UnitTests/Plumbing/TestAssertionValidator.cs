using System.Security.Claims;
using System.Threading.Tasks;
using Thinktecture.IdentityModel;
using Thinktecture.IdentityServer.Core.Connect;
using Thinktecture.IdentityServer.Core.Connect.Services;

namespace UnitTests.Plumbing
{
    class TestAssertionValidator : IAssertionGrantValidator
    {
        public async Task<ClaimsPrincipal> ValidateAsync(ValidatedTokenRequest request)
        {
            if (request.GrantType == "assertionType" && request.Assertion == "assertion")
            {
                return Principal.Create("Assertion", new Claim("sub", "bob"));
            };

            return null;
        }
    }
}