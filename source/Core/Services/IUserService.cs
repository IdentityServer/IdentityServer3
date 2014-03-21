using System.Collections.Generic;
using System.Security.Claims;

namespace Thinktecture.IdentityServer.Core.Services
{
    public class AuthenticateResult
    {
        public string Subject { get; set; }
        public string Username { get; set; }
    }

    public interface IUserService
    {
        AuthenticateResult Authenticate(string username, string password);
        AuthenticateResult Authenticate(string subject, IEnumerable<Claim> claims);
        IEnumerable<Claim> GetProfileData(string subject, IEnumerable<string> requestedClaimTypes = null);
    }
}
