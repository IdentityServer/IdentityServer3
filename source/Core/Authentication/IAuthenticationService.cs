using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Thinktecture.IdentityServer.Core.Authentication
{
    public interface IAuthenticationService
    {
        IEnumerable<Claim> Authenticate(string username, string password);
    }
    public class AuthenticationService : IAuthenticationService
    {
        public IEnumerable<Claim> Authenticate(string username, string password)
        {
            return new Claim[] { 
                new Claim(ClaimTypes.Role, "Admin"),
            };
        }
    }

}
