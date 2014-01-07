using System.Collections.Generic;
using System.Security.Claims;

namespace Thinktecture.IdentityServer.Core.Services
{
    public interface IProfileService
    {
        IEnumerable<Claim> GetProfileData(string sub, IEnumerable<string> requestedClaimTypes = null);
    }
}
