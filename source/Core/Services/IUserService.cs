using System.Collections.Generic;
using System.Security.Claims;

namespace Thinktecture.IdentityServer.Core.Services
{
    public class AuthenticateResult
    {
        public string Subject { get; set; }
        public string Name { get; set; }
    }

    public interface IUserService
    {
        //public bool PromptUserToLinkExternalAccountsWhenSignedIn { get; set; }
        AuthenticateResult AuthenticateLocal(string username, string password);
        //AuthenticateResult ExternalAuthenticate(string subject, IEnumerable<Claim> claims);
        AuthenticateResult AuthenticateExternal(IEnumerable<Claim> claims);
        //AuthenticateResult LinkExternal(string subject, string provider, string providerId, IEnumerable<Claim> claims);
        // AuthenticateResult Authenticate(string provider, string providerId, IEnumerable<Claim> claims);
        IEnumerable<Claim> GetProfileData(string subject, IEnumerable<string> requestedClaimTypes = null);
    }

    //public interface IUserServiceCustomization
    //{
    //    // IEnumerable<Claim> CreateFromExternal(IEnumerable<Claim> externalClaims)
    //    // IEnumerable<Claim> UpdateFromExternal(IEnumerable<Claim> currentClaims, IEnumerable<Claim> externalClaims)
    //    // email --> column
    //    // phone --> columns
    //    // Role: Admin
    //    // Role: Mgr
    //}

    //public interface IUserStore
    //{
    //    // create
    //}

  
}
