using System;
using System.Collections.Generic;
using System.Security.Claims;

namespace Thinktecture.IdentityServer.Core.Services
{
    public class AuthenticateResult
    {
        public AuthenticateResult(string subject, string name)
        {
            if (String.IsNullOrWhiteSpace(subject)) throw new ArgumentNullException("subject");
            if (String.IsNullOrWhiteSpace(name)) throw new ArgumentNullException("name");

            this.Subject = subject;
            this.Name = name;
        }
        public AuthenticateResult(string errorMessage)
        {
            if (String.IsNullOrWhiteSpace(errorMessage)) throw new ArgumentNullException("errorMessage");

            this.ErrorMessage = errorMessage;
        }

        public string Subject { get; private set; }
        public string Name { get; private set; }
        public string ErrorMessage { get; private set; }
        public bool IsError
        {
            get { return !String.IsNullOrWhiteSpace(this.ErrorMessage); }
        }
    }

    public class ExternalAuthenticateResult : AuthenticateResult
    {
        public ExternalAuthenticateResult(string subject, string name, string provider)
            : base(subject, name)
        {
            if (String.IsNullOrWhiteSpace(provider)) throw new ArgumentNullException("provider");

            this.Provider = provider;
        }
        
        public ExternalAuthenticateResult(string errorMessage)
            : base(errorMessage)
        {
        }

        public string Provider { get; private set; }
    }

    public interface IUserService
    {
        //public bool PromptUserToLinkExternalAccountsWhenSignedIn { get; set; }
        AuthenticateResult AuthenticateLocal(string username, string password);
        //AuthenticateResult ExternalAuthenticate(string subject, IEnumerable<Claim> claims);
        ExternalAuthenticateResult AuthenticateExternal(IEnumerable<Claim> claims);
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
