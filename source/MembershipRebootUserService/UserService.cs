using BrockAllen.MembershipReboot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using Thinktecture.IdentityModel.Extensions;
using Thinktecture.IdentityServer.Core;
using Thinktecture.IdentityServer.Core.Services;

namespace MembershipReboot.IdentityServer
{
    public class UserService : IUserService, IDisposable
    {
        UserAccountService userAccountService;
        IDisposable cleanup;
        public UserService(UserAccountService userAccountService, IDisposable cleanup)
        {
            if (userAccountService == null) throw new ArgumentNullException("userAccountService");

            this.userAccountService = userAccountService;
            this.cleanup = cleanup;
        }

        public void Dispose()
        {
            if (this.cleanup != null)
            {
                this.cleanup.Dispose();
                this.cleanup = null;
            }
        }

        public AuthenticateResult AuthenticateLocal(string username, string password)
        {
            UserAccount acct;
            if (userAccountService.Authenticate(username, password, out acct))
            {
                if (acct.RequiresPasswordReset)
                {
                    return new AuthenticateResult("You must reset your password");
                }
                if (!acct.IsLoginAllowed)
                {
                    return new AuthenticateResult("/core/foo", acct.ID.ToString("D"), acct.Username);
                }

                return new AuthenticateResult(acct.ID.ToString("D"), acct.Username);
            }

            return null;
        }

        public ExternalAuthenticateResult AuthenticateExternal(IEnumerable<Claim> claims)
        {
            if (claims == null)
            {
                return null;
            }

            var subClaim = claims.FirstOrDefault(x => x.Type == Constants.ClaimTypes.Subject);
            if (subClaim == null)
            {
                subClaim = claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier);

                if (subClaim == null)
                {
                    return null;
                }
            }

            var provider = subClaim.Issuer;
            var providerId = subClaim.Value;

            var acct = this.userAccountService.GetByLinkedAccount(provider, providerId);
            if (acct == null)
            {
                acct = userAccountService.CreateAccount(Guid.NewGuid().ToString("N"), null, null);
            }

            userAccountService.AddOrUpdateLinkedAccount(acct, provider, providerId, claims);

            string displayName = null;
            if (acct.HasPassword()) displayName = acct.Username;

            if (displayName == null) acct.GetClaimValue(Constants.ClaimTypes.PreferredUserName);
            if (displayName == null) displayName = acct.GetClaimValue(Constants.ClaimTypes.Name);
            if (displayName == null) displayName = acct.GetClaimValue(ClaimTypes.Name);
            if (displayName == null) displayName = acct.GetClaimValue(Constants.ClaimTypes.Email);
            if (displayName == null) displayName = acct.GetClaimValue(ClaimTypes.Email);

            if (displayName == null) displayName = Thinktecture.IdentityModel.Extensions.ClaimsExtensions.GetValue(claims, Constants.ClaimTypes.PreferredUserName);
            if (displayName == null) displayName = Thinktecture.IdentityModel.Extensions.ClaimsExtensions.GetValue(claims, Constants.ClaimTypes.Name);
            if (displayName == null) displayName = Thinktecture.IdentityModel.Extensions.ClaimsExtensions.GetValue(claims, ClaimTypes.Name);
            if (displayName == null) displayName = Thinktecture.IdentityModel.Extensions.ClaimsExtensions.GetValue(claims, Constants.ClaimTypes.Email);
            if (displayName == null) displayName = Thinktecture.IdentityModel.Extensions.ClaimsExtensions.GetValue(claims, ClaimTypes.Email);

            displayName = displayName ?? acct.Username;

            return new ExternalAuthenticateResult(provider, acct.ID.ToString("D"), displayName);
        }

        public IEnumerable<System.Security.Claims.Claim> GetProfileData(string subject,
            IEnumerable<string> requestedClaimTypes = null)
        {
            var acct = userAccountService.GetByID(subject.ToGuid());
            if (acct == null)
            {
                throw new ArgumentException("Invalid subject identifier");
            }

            if (requestedClaimTypes == null)
            {
                return null;
            }

            var claims = new List<Claim>{
                new Claim(Constants.ClaimTypes.Subject, acct.ID.ToString("D")),
                new Claim(Constants.ClaimTypes.Name, acct.Username),
                new Claim(Constants.ClaimTypes.UpdatedAt, acct.LastUpdated.ToEpochTime().ToString()),
            };
            if (!String.IsNullOrWhiteSpace(acct.Email))
            {
                claims.Add(new Claim(Constants.ClaimTypes.Email, acct.Email));
                claims.Add(new Claim(Constants.ClaimTypes.EmailVerified, acct.IsAccountVerified ? "true" : "false"));
            }
            if (!String.IsNullOrWhiteSpace(acct.MobilePhoneNumber))
            {
                claims.Add(new Claim(Constants.ClaimTypes.PhoneNumber, acct.MobilePhoneNumber));
                claims.Add(new Claim(Constants.ClaimTypes.PhoneNumberVerified, !String.IsNullOrWhiteSpace(acct.MobilePhoneNumber) ? "true" : "false"));
            }
            claims.AddRange(acct.Claims.Select(x => new Claim(x.Type, x.Value)));

            return claims;//.Where(x => requestedClaimTypes.Contains(x.Type));
        }
    }
}
