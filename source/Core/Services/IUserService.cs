using Microsoft.Owin;
using System;
using System.Collections.Generic;
using System.Security.Claims;

namespace Thinktecture.IdentityServer.Core.Services
{
    public class AuthenticateResult
    {
        public AuthenticateResult(string errorMessage)
        {
            if (String.IsNullOrWhiteSpace(errorMessage)) throw new ArgumentNullException("errorMessage");

            this.ErrorMessage = errorMessage;
        }

        public AuthenticateResult(string subject, string name)
        {
            if (String.IsNullOrWhiteSpace(subject)) throw new ArgumentNullException("subject");
            if (String.IsNullOrWhiteSpace(name)) throw new ArgumentNullException("name");

            this.Subject = subject;
            this.Name = name;
        }

        // TODO: maybe this should be a PathString?
        public AuthenticateResult(string redirectPath, string subject = null, string name = null)
        {
            if (String.IsNullOrWhiteSpace(redirectPath)) throw new ArgumentNullException("redirectPath");
            if (!String.IsNullOrWhiteSpace(subject) && String.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentNullException("name");
            }
            if (String.IsNullOrWhiteSpace(subject) && !String.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentNullException("subject");
            }

            this.RedirectPath = new PathString(redirectPath);
            this.Subject = subject;
            this.Name = name;
        }

        public string ErrorMessage { get; private set; }
        public bool IsError
        {
            get { return !String.IsNullOrWhiteSpace(this.ErrorMessage); }
        }

        public string Subject { get; private set; }
        public string Name { get; private set; }

        public PathString RedirectPath { get; private set; }
        public bool IsRedirect
        {
            get
            {
                return RedirectPath.HasValue;
            }
        }
    }

    public class ExternalAuthenticateResult : AuthenticateResult
    {
        public ExternalAuthenticateResult(string errorMessage)
            : base(errorMessage)
        {
        }
        
        public ExternalAuthenticateResult(string provider, string subject, string name)
            : base(subject, name)
        {
            if (String.IsNullOrWhiteSpace(provider)) throw new ArgumentNullException("provider");

            this.Provider = provider;
        }

        public ExternalAuthenticateResult(string redirectPath, string provider = null, string subject = null, string name = null)
            : base(redirectPath, subject, name)
        {
            if (!String.IsNullOrWhiteSpace(provider) && String.IsNullOrWhiteSpace(subject))
            {
                throw new ArgumentNullException("subject");
            }
            if (String.IsNullOrWhiteSpace(provider) && !String.IsNullOrWhiteSpace(subject))
            {
                throw new ArgumentNullException("provider");
            }
            
            this.Provider = provider;
        }

        public string Provider { get; private set; }
    }

    public interface IUserService
    {
        AuthenticateResult AuthenticateLocal(string username, string password);
        ExternalAuthenticateResult AuthenticateExternal(IEnumerable<Claim> claims);
        IEnumerable<Claim> GetProfileData(string subject, IEnumerable<string> requestedClaimTypes = null);
    }

    public interface IUserServiceCustomization
    {
        // IEnumerable<Claim> CreateFromExternal(IEnumerable<Claim> externalClaims)
        // IEnumerable<Claim> UpdateFromExternal(IEnumerable<Claim> currentClaims, IEnumerable<Claim> externalClaims)
        // email --> column
        // phone --> columns
        // Role: Admin
        // Role: Mgr
    }
}
