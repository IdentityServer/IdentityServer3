/*
 * Copyright (c) Dominick Baier, Brock Allen.  All rights reserved.
 * see license
 */

using BrockAllen.MembershipReboot;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Thinktecture.IdentityModel.Extensions;
using Thinktecture.IdentityServer.Core;
using Thinktecture.IdentityServer.Core.Services;
using ClaimHelper = BrockAllen.MembershipReboot.ClaimsExtensions;

namespace MembershipReboot.IdentityServer
{
    public class MembershipRebootUserService : IUserService, IDisposable
    {
        protected readonly UserAccountService userAccountService;
        IDisposable cleanup;
        public MembershipRebootUserService(UserAccountService userAccountService, IDisposable cleanup)
        {
            if (userAccountService == null) throw new ArgumentNullException("userAccountService");

            this.userAccountService = userAccountService;
            this.cleanup = cleanup;
        }

        public virtual void Dispose()
        {
            if (this.cleanup != null)
            {
                this.cleanup.Dispose();
                this.cleanup = null;
            }
        }

        public virtual async Task<IEnumerable<System.Security.Claims.Claim>> GetProfileDataAsync(string subject,
            IEnumerable<string> requestedClaimTypes = null)
        {
            var acct = userAccountService.GetByID(subject.ToGuid());
            if (acct == null)
            {
                throw new ArgumentException("Invalid subject identifier");
            }

            var claims = GetClaimsFromAccount(acct);
            if (requestedClaimTypes != null)
            {
                claims = claims.Where(x => requestedClaimTypes.Contains(x.Type));
            }
            return claims;
        }

        protected virtual IEnumerable<Claim> GetClaimsFromAccount(UserAccount account)
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
            claims.AddRange(account.LinkedAccountClaims.Select(x => new Claim(x.Type, x.Value)));

            return claims;
        }
        
        public virtual async Task<AuthenticateResult> AuthenticateLocalAsync(string username, string password)
        {
            UserAccount account;
            if (userAccountService.Authenticate(username, password, out account))
            {
                var subject = account.ID.ToString("D");
                var name = account.Username;

                if (account.RequiresTwoFactorAuthCodeToSignIn())
                {
                    return new AuthenticateResult("/core/account/twofactor", subject, name);
                }
                if (account.RequiresTwoFactorCertificateToSignIn())
                {
                    return new AuthenticateResult("/core/account/certificate", subject, name);
                }
                if (account.RequiresPasswordReset)
                {
                    return new AuthenticateResult("/core/account/changepassword", subject, name);
                }

                return new AuthenticateResult(subject, name);
            }

            if (account != null)
            {
                if (!account.IsLoginAllowed)
                {
                    return new AuthenticateResult("Account is not allowed to login");
                }

                if (account.IsAccountClosed)
                {
                    return new AuthenticateResult("Account is closed");
                }
            }

            return null;
        }

        public virtual async Task<ExternalAuthenticateResult> AuthenticateExternalAsync(string subject, IEnumerable<Claim> externalClaims)
        {
            if (externalClaims == null || !externalClaims.Any())
            {
                return null;
            }

            var subClaim = externalClaims.FirstOrDefault(x => x.Type == Constants.ClaimTypes.Subject);
            if (subClaim == null)
            {
                subClaim = externalClaims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier);

                if (subClaim == null)
                {
                    return null;
                }
            }

            externalClaims = externalClaims.Except(new Claim[] { subClaim });
            externalClaims = NormalizeExternalClaimTypes(externalClaims);

            var provider = subClaim.Issuer;
            var providerId = subClaim.Value;

            try
            {
                var acct = this.userAccountService.GetByLinkedAccount(provider, providerId);
                if (acct == null)
                {
                    return await ProcessNewExternalAccountAsync(provider, providerId, externalClaims);
                }
                else
                {
                    return await ProcessExistingExternalAccountAsync(acct.ID, provider, providerId, externalClaims);
                }
            }
            catch (ValidationException ex)
            {
                return new ExternalAuthenticateResult(ex.Message);
            }
        }

        protected virtual async Task<ExternalAuthenticateResult> ProcessNewExternalAccountAsync(string provider, string providerId, IEnumerable<Claim> claims)
        {
            var acct = userAccountService.CreateAccount(Guid.NewGuid().ToString("N"), null, null);

            var result = await AccountCreatedFromExternalProviderAsync(acct.ID, provider, providerId, claims);
            if (result != null) return result;

            return await SignInFromExternalProviderAsync(acct.ID, provider);
        }

        protected virtual async Task<ExternalAuthenticateResult> AccountCreatedFromExternalProviderAsync(Guid accountID, string provider, string providerId, IEnumerable<Claim> claims)
        {
            claims = FilterExternalClaimsForNewAccount(provider, claims);

            var result = SetNewAccountUsername(accountID, provider, ref claims);
            if (result != null) return result;
            
            result= SetNewAccountEmail(accountID, provider, ref claims);
            if (result != null) return result;

            result = SetNewAccountPhone(accountID, provider, ref claims);
            if (result != null) return result;
            
            return await UpdateAccountFromExternalClaimsAsync(accountID, provider, providerId, claims);
        }

        protected virtual async Task<ExternalAuthenticateResult> SignInFromExternalProviderAsync(Guid accountID, string provider)
        {
            var acct = userAccountService.GetByID(accountID);
            return new ExternalAuthenticateResult(provider, accountID.ToString("D"), GetNameForAccount(accountID));
        }

        protected virtual async Task<ExternalAuthenticateResult> ProcessExistingExternalAccountAsync(Guid accountID, string provider, string providerId, IEnumerable<Claim> claims)
        {
            claims = FilterExternalClaimsForExistingAccount(accountID, provider, claims); 

            SetAccountEmail(accountID, ref claims);
            SetAccountPhone(accountID, ref claims);

            var result = await UpdateAccountFromExternalClaimsAsync(accountID, provider, providerId, claims);
            if (result != null) return result;

            return await SignInFromExternalProviderAsync(accountID, provider);
        }

        protected virtual async Task<ExternalAuthenticateResult> UpdateAccountFromExternalClaimsAsync(Guid accountID, string provider, string providerId, IEnumerable<Claim> claims)
        {
            var account = userAccountService.GetByID(accountID);
            userAccountService.AddOrUpdateLinkedAccount(account, provider, providerId, claims);

            return null;
        }

        protected virtual IEnumerable<Claim> FilterExternalClaimsForNewAccount(string provider, IEnumerable<Claim> claims)
        {
            return claims;
        }

        protected virtual IEnumerable<Claim> FilterExternalClaimsForExistingAccount(Guid accountID, string provider, IEnumerable<Claim> claims)
        {
            return claims;
        }

        protected virtual ExternalAuthenticateResult SetNewAccountUsername(Guid accountID, string provider, ref IEnumerable<Claim> claims)
        {
            var name = ClaimHelper.GetValue(claims, Constants.ClaimTypes.PreferredUserName);
            if (name == null) name = ClaimHelper.GetValue(claims, Constants.ClaimTypes.Name);
            
            if (name != null)
            {
                var acct = userAccountService.GetByID(accountID);
                try
                {
                    userAccountService.ChangeUsername(acct.ID, name);
                    var nameClaims = new string[] { Constants.ClaimTypes.PreferredUserName, Constants.ClaimTypes.Name };
                    claims = claims.Where(x => !nameClaims.Contains(x.Type));
                }
                catch (ValidationException ex)
                {
                    // presumably the name is already associated with another account or invalid
                    // so let's register the user
                    //return new ExternalAuthenticateResult("/core/account/register", 
                    //    provider, 
                    //    accountID.ToString("D"), 
                    //    GetNameForAccount(accountID));
                }
            }

            return null;
        }

        protected virtual ExternalAuthenticateResult SetNewAccountEmail(Guid accountID, string provider, ref IEnumerable<Claim> claims)
        {
            SetAccountEmail(accountID, ref claims);
            return null;
        }

        protected virtual void SetAccountEmail(Guid accountID, ref IEnumerable<Claim> claims)
        {
            var email = ClaimHelper.GetValue(claims, Constants.ClaimTypes.Email);
            if (email != null)
            {
                var acct = userAccountService.GetByID(accountID);
                if (acct.Email == null)
                {
                    try
                    {
                        var email_verified = ClaimHelper.GetValue(claims, Constants.ClaimTypes.EmailVerified);
                        if (email_verified != null && email_verified == "true")
                        {
                            userAccountService.SetConfirmedEmail(acct.ID, email);
                        }
                        else
                        {
                            userAccountService.ChangeEmailRequest(acct.ID, email);
                        }

                        var emailClaims = new string[] { Constants.ClaimTypes.Email, Constants.ClaimTypes.EmailVerified };
                        claims = claims.Where(x => !emailClaims.Contains(x.Type));
                    }
                    catch (ValidationException ex)
                    {
                        // presumably the email is already associated with another account
                        // so eat the validation exception and let the claim pass thru
                    }
                }
            }
        }

        protected virtual ExternalAuthenticateResult SetNewAccountPhone(Guid accountID, string provider, ref IEnumerable<Claim> claims)
        {
            SetAccountPhone(accountID, ref claims);
            return null;
        }

        protected virtual void SetAccountPhone(Guid accountID, ref IEnumerable<Claim> claims)
        {
            var phone = ClaimHelper.GetValue(claims, Constants.ClaimTypes.PhoneNumber);
            if (phone != null)
            {
                var acct = userAccountService.GetByID(accountID);
                if (acct.MobilePhoneNumber == null)
                {
                    try
                    {
                        var phone_verified = ClaimHelper.GetValue(claims, Constants.ClaimTypes.PhoneNumberVerified);
                        if (phone_verified != null && phone_verified == "true")
                        {
                            userAccountService.SetConfirmedMobilePhone(acct.ID, phone);
                        }
                        else
                        {
                            userAccountService.ChangeMobilePhoneRequest(acct.ID, phone);
                        }

                        var phoneClaims = new string[] { Constants.ClaimTypes.PhoneNumber, Constants.ClaimTypes.PhoneNumberVerified };
                        claims = claims.Where(x => !phoneClaims.Contains(x.Type));
                    }
                    catch (ValidationException ex)
                    {
                        // presumably the phone is already associated with another account
                        // so eat the validation exception and let the claim pass thru
                    }
                }
            }
        }

        protected virtual string GetNameForAccount(Guid accountID)
        {
            var acct = userAccountService.GetByID(accountID);
            string name = null;
            
            // if the user has a local password, then presumably they know
            // their own username and will recognize it (rather than a generated one)
            if (acct.HasPassword()) name = acct.Username;

            if (name == null) name = acct.GetClaimValue(Constants.ClaimTypes.PreferredUserName);
            if (name == null) name = acct.GetClaimValue(Constants.ClaimTypes.Name);
            if (name == null) name = acct.GetClaimValue(ClaimTypes.Name);

            if (name == null)
            {
                name = acct.LinkedAccountClaims
                    .Where(x =>
                        x.Type == Constants.ClaimTypes.PreferredUserName ||
                        x.Type == Constants.ClaimTypes.Name ||
                        x.Type == ClaimTypes.Name)
                    .Select(x=>x.Value)
                    .FirstOrDefault();
            }

            name = name ?? acct.Username;
            return name;
        }

        protected virtual IEnumerable<Claim> NormalizeExternalClaimTypes(IEnumerable<Claim> incomingClaims)
        {
            return Thinktecture.IdentityServer.Core.Plumbing.ClaimMap.Map(incomingClaims);
        }
    }
}
