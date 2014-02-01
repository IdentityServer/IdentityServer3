using System.Collections.Generic;
using System.Security.Claims;
using Thinktecture.IdentityServer.Core.Services;

namespace Thinktecture.IdentityServer.Core.TestServices
{
    public class TestUserService : IUserService
    {
        public IEnumerable<Claim> GetProfileData(string sub, IEnumerable<string> requestedClaimTypes = null)
        {
            return new List<Claim>
            {
                new Claim(Constants.ClaimTypes.Subject, sub)
            };
        }

        public string Authenticate(string username, string password)
        {
            return username;
        }
    }
}
