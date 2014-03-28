using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Thinktecture.IdentityServer.Core.Services;

namespace UnitTests.Plumbing
{
    class TestUserService : IUserService
    {
        public async Task<AuthenticateResult> AuthenticateLocalAsync(string username, string password)
        {
            if (username == password)
            {
                return new AuthenticateResult(username, username);
            }

            return null;
        }

        public async Task<IEnumerable<System.Security.Claims.Claim>> GetProfileDataAsync(string sub, IEnumerable<string> requestedClaimTypes = null)
        {
            throw new NotImplementedException();
        }


        public async Task<ExternalAuthenticateResult> AuthenticateExternalAsync(string subject, IEnumerable<System.Security.Claims.Claim> claims)
        {
            throw new NotImplementedException();
        }
    }
}
