using System.Collections.Generic;
using System.Security.Claims;
using Thinktecture.IdentityServer.Core.Services;

namespace Thinktecture.IdentityServer.Core.TestServices
{
    public class TestUserService : IUserService
    {
        public IEnumerable<Claim> GetProfileData(string sub, IEnumerable<string> requestedClaimTypes = null)
        {
            var claims = new List<Claim>();

            if (requestedClaimTypes == null)
            {
                claims.Add(new Claim(Constants.ClaimTypes.Subject, sub));
                return claims;
            }

            foreach (var requestedClaim in requestedClaimTypes)
            {
                if (requestedClaim == Constants.ClaimTypes.Subject)
                {
                    claims.Add(new Claim(Constants.ClaimTypes.Subject, sub));
                }
                else
                {
                    claims.Add(new Claim(requestedClaim, requestedClaim));
                }
            }

            return claims;
        }

        public string Authenticate(string username, string password)
        {
            return username;
        }
    }
}
