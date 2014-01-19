using System.Collections.Generic;
using System.Security.Claims;

namespace Thinktecture.IdentityServer.Core.Authentication
{
    public interface IAuthenticationService
    {
        string Authenticate(string username, string password);
    }
}
