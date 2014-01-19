using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Thinktecture.IdentityServer.Core.Authentication;

namespace Thinktecture.IdentityServer.Core.TestServices
{
    public class TestAuthenticationService : IAuthenticationService
    {
        public string Authenticate(string username, string password)
        {
            return username;
        }
    }
}
