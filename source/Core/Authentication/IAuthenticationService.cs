using System.Collections.Generic;
using System.Security.Claims;

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
