using System.Collections.Generic;
using System.Security.Claims;

namespace Thinktecture.IdentityServer.Core.Services
{
    public interface IUserService
    {
        string Authenticate(string username, string password);
        IEnumerable<Claim> GetProfileData(string sub, IEnumerable<string> requestedClaimTypes = null);
    }
}
