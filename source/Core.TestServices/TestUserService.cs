using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Thinktecture.IdentityServer.Core;
using Thinktecture.IdentityServer.Core.Services;

namespace Thinktecture.IdentityServer.TestServices
{
    public class TestUserService : IUserService
    {
        public async Task<IEnumerable<Claim>> GetProfileDataAsync(string sub, IEnumerable<string> requestedClaimTypes = null)
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

        public async Task<AuthenticateResult> AuthenticateLocalAsync(string username, string password)
        {
            if (username != password) return null;

            return new AuthenticateResult(username, username);
        }


        public async Task<ExternalAuthenticateResult> AuthenticateExternalAsync(string subject, IEnumerable<Claim> claims)
        {
            if (claims == null)
            {
                return null;
            }

            var name = claims.FirstOrDefault(x=>x.Type==ClaimTypes.NameIdentifier);
            if (name == null)
            {
                return null;
            }

            return new ExternalAuthenticateResult("external", name.Value, name.Value);
        }
    }
}
