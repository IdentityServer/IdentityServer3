using System;
using System.Collections.Generic;
using Thinktecture.IdentityServer.Core.Services;

namespace UnitTests.Plumbing
{
    class TestUserService : IUserService
    {
        public AuthenticateResult AuthenticateLocal(string username, string password)
        {
            if (username == password)
            {
                return new AuthenticateResult(username, username);
            }

            return null;
        }

        public IEnumerable<System.Security.Claims.Claim> GetProfileData(string sub, IEnumerable<string> requestedClaimTypes = null)
        {
            throw new NotImplementedException();
        }


        public ExternalAuthenticateResult AuthenticateExternal(string subject, IEnumerable<System.Security.Claims.Claim> claims)
        {
            throw new NotImplementedException();
        }
    }
}
