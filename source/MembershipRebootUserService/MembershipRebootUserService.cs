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
    public class MembershipRebootUserService : IUserService, IDisposable
    {
        UserAccountService userAccountService;
        IDisposable cleanup;
        public MembershipRebootUserService(UserAccountService userAccountService, IDisposable cleanup)
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
                var subject = acct.ID.ToString("D");
                var name = acct.Username;

                if (acct.RequiresPasswordReset)
                {
                    return new AuthenticateResult("You must reset your password");
                }
                
                if (!acct.IsLoginAllowed)
                {
                    return new AuthenticateResult("/core/foo?bar=baz", subject, name);
                }

                return new AuthenticateResult(subject, name);
            }

            return null;
        }

        public ExternalAuthenticateResult AuthenticateExternal(string subject, IEnumerable<Claim> claims)
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

            string name = GetNameFromExternalLogin(acct, claims);

            return new ExternalAuthenticateResult(provider, acct.ID.ToString("D"), name);
        }

        private static string GetNameFromExternalLogin(UserAccount acct, IEnumerable<Claim> claims)
        {
            string name = null;
            if (acct.HasPassword()) name = acct.Username;

            if (name == null) name = acct.GetClaimValue(Constants.ClaimTypes.PreferredUserName);
            if (name == null) name = acct.GetClaimValue(Constants.ClaimTypes.Name);
            if (name == null) name = acct.GetClaimValue(ClaimTypes.Name);
            if (name == null) name = acct.GetClaimValue(Constants.ClaimTypes.Email);
            if (name == null) name = acct.GetClaimValue(ClaimTypes.Email);

            if (name == null) name = Thinktecture.IdentityModel.Extensions.ClaimsExtensions.GetValue(claims, Constants.ClaimTypes.PreferredUserName);
            if (name == null) name = Thinktecture.IdentityModel.Extensions.ClaimsExtensions.GetValue(claims, Constants.ClaimTypes.Name);
            if (name == null) name = Thinktecture.IdentityModel.Extensions.ClaimsExtensions.GetValue(claims, ClaimTypes.Name);
            if (name == null) name = Thinktecture.IdentityModel.Extensions.ClaimsExtensions.GetValue(claims, Constants.ClaimTypes.Email);
            if (name == null) name = Thinktecture.IdentityModel.Extensions.ClaimsExtensions.GetValue(claims, ClaimTypes.Email);

            name = name ?? acct.Username;
            return name;
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

            var claims = GetClaimsFromAccount(acct);
            return claims;//.Where(x => requestedClaimTypes.Contains(x.Type));
        }

        IEnumerable<Claim> GetClaimsFromAccount(UserAccount account)
        {
            var claims = new List<Claim>{
                new Claim(Constants.ClaimTypes.Subject, account.ID.ToString("D")),
                new Claim(Constants.ClaimTypes.Name, account.Username),
                new Claim(Constants.ClaimTypes.UpdatedAt, account.LastUpdated.ToEpochTime().ToString()),
            };
            if (!String.IsNullOrWhiteSpace(account.Email))
            {
                claims.Add(new Claim(Constants.ClaimTypes.Email, account.Email));
                claims.Add(new Claim(Constants.ClaimTypes.EmailVerified, account.IsAccountVerified ? "true" : "false"));
            }
            if (!String.IsNullOrWhiteSpace(account.MobilePhoneNumber))
            {
                claims.Add(new Claim(Constants.ClaimTypes.PhoneNumber, account.MobilePhoneNumber));
                claims.Add(new Claim(Constants.ClaimTypes.PhoneNumberVerified, !String.IsNullOrWhiteSpace(account.MobilePhoneNumber) ? "true" : "false"));
            }
            claims.AddRange(account.Claims.Select(x => new Claim(x.Type, x.Value)));

            return claims;
        }

        //IEnumerable<Claim> NormalizeFromWIFClaims(IEnumerable<Claim> claims)
        //{
        //    return claims;
        //}
        
        //IEnumerable<Claim> NormalizeFromJWTClaims(IEnumerable<Claim> claims)
        //{
        //    return claims;
        //}

        //void UpdateAccountFromClaims(UserAccount account, IEnumerable<Claim> claims)
        //{
        //    var typesThatAreNotGeneral = new[]{
        //        Constants.ClaimTypes.Subject, 
        //        Constants.ClaimTypes.Name, 
        //        Constants.ClaimTypes.Email,
        //        Constants.ClaimTypes.EmailVerified,
        //        Constants.ClaimTypes.PhoneNumber,
        //        Constants.ClaimTypes.PhoneNumberVerified,
        //    };

        //    var oldGeneralClaims = account.Claims.Select(x => new Tuple<string, string>(x.Type, x.Value));
        //    var newGeneralClaims = claims.Where(x => !typesThatAreNotGeneral.Contains(x.Type)).Select(x => new Tuple<string, string>(x.Type, x.Value));
        //    var intersection = newGeneralClaims.Intersect(oldGeneralClaims);
        //}

        //void CalculateDelta(
        //    IEnumerable<Tuple<string, string>> oldClaims,
        //    IEnumerable<Tuple<string, string>> newClaims,
        //    out IEnumerable<Tuple<string, string>> claimsToAdd,
        //    out IEnumerable<Tuple<string, string>> claimsToRemove)
        //{
        //    var intersection = newClaims.Intersect(oldClaims);
        //    claimsToAdd = newClaims.Except(intersection);
        //    claimsToRemove = oldClaims.Except(intersection);
        //}


    }
}
