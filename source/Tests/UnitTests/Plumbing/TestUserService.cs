using System;
using System.Collections.Generic;
using Thinktecture.IdentityServer.Core.Services;

namespace UnitTests.Plumbing
{
    class TestUserService : IUserService
    {
        public string Authenticate(string username, string password)
        {
            if (username == password)
            {
                return username;
            }

            return null;
        }

        public IEnumerable<System.Security.Claims.Claim> GetProfileData(string sub, IEnumerable<string> requestedClaimTypes = null)
        {
            throw new NotImplementedException();
        }
    }
}
