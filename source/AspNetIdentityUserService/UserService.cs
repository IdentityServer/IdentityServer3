/*
 * Copyright (c) Dominick Baier, Brock Allen.  All rights reserved.
 * see license
 */
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Thinktecture.IdentityServer.Core.Authentication;
using Thinktecture.IdentityServer.Core.Models;
using Thinktecture.IdentityServer.Core.Services;

namespace Thinktecture.IdentityServer.AspNetIdentity
{
    public class UserService<TUser, TKey> : IUserService, IDisposable
        where TUser : class, IUser<TKey>, new()
        where TKey : IEquatable<TKey>
    {
        protected readonly UserManager<TUser, TKey> userManager;
        IDisposable cleanup;

        protected readonly Func<string, TKey> ConvertSubjectToKey;
        
        public UserService(UserManager<TUser, TKey> userManager, IDisposable cleanup)
        {
            if (userManager == null) throw new ArgumentNullException("userManager");
            
            this.userManager = userManager;
            this.cleanup = cleanup;

            var keyType = typeof(TKey);
            if (keyType == typeof(string)) ConvertSubjectToKey = subject => (TKey)ParseString(subject);
            else if (keyType == typeof(int)) ConvertSubjectToKey = subject => (TKey)ParseInt(subject);
            else if (keyType == typeof(long)) ConvertSubjectToKey = subject => (TKey)ParseLong(subject);
            else if (keyType == typeof(Guid)) ConvertSubjectToKey = subject => (TKey)ParseGuid(subject);
            else
            {
                throw new InvalidOperationException("Key type not supported");
            }
        }

        object ParseString(string sub)
        {
            return sub;
        }
        object ParseInt(string sub)
        {
            int key;
            if (!Int32.TryParse(sub, out key)) return 0;
            return key;
        }
        object ParseLong(string sub)
        {
            long key;
            if (!Int64.TryParse(sub, out key)) return 0;
            return key;
        }
        object ParseGuid(string sub)
        {
            Guid key;
            if (!Guid.TryParse(sub, out key)) return Guid.Empty;
            return key;
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
            TKey key = ConvertSubjectToKey(subject);
            var acct = await userManager.FindByIdAsync(key);
            if (acct == null)
            {
                throw new ArgumentException("Invalid subject identifier");
            }

            var claims = await GetClaimsFromAccount(acct);
            if (requestedClaimTypes != null)
            {
                claims = claims.Where(x => requestedClaimTypes.Contains(x.Type));
            }
            return claims;
        }

        protected virtual async Task<IEnumerable<Claim>> GetClaimsFromAccount(TUser user)
        {
            var claims = new List<Claim>{
                new Claim(Thinktecture.IdentityServer.Core.Constants.ClaimTypes.Subject, user.Id.ToString()),
                new Claim(Thinktecture.IdentityServer.Core.Constants.ClaimTypes.Name, await GetDisplayNameForAccountAsync(user.Id)),
            };

            if (userManager.SupportsUserEmail)
            {
                var email = await userManager.GetEmailAsync(user.Id);
                if (!String.IsNullOrWhiteSpace(email))
                {
                    claims.Add(new Claim(Thinktecture.IdentityServer.Core.Constants.ClaimTypes.Email, email));
                    var verified = await userManager.IsEmailConfirmedAsync(user.Id);
                    claims.Add(new Claim(Thinktecture.IdentityServer.Core.Constants.ClaimTypes.EmailVerified, verified ? "true" : "false"));
                }
            }

            if (userManager.SupportsUserPhoneNumber)
            {
                var phone = await userManager.GetPhoneNumberAsync(user.Id);
                if (!String.IsNullOrWhiteSpace(phone))
                {
                    claims.Add(new Claim(Thinktecture.IdentityServer.Core.Constants.ClaimTypes.PhoneNumber, phone));
                    var verified = await userManager.IsPhoneNumberConfirmedAsync(user.Id);
                    claims.Add(new Claim(Thinktecture.IdentityServer.Core.Constants.ClaimTypes.PhoneNumberVerified, verified ? "true" : "false"));
                }
            }

            if (userManager.SupportsUserClaim)
            {
                claims.AddRange(await userManager.GetClaimsAsync(user.Id));
            }

            if (userManager.SupportsUserRole)
            {
                var roleClaims =
                    from role in await userManager.GetRolesAsync(user.Id)
                    select new Claim(ClaimTypes.Role, role);
                claims.AddRange(roleClaims);
            }

            return claims;
        }

        protected virtual async Task<string> GetDisplayNameForAccountAsync(TKey userID)
        {
            var claims = await userManager.GetClaimsAsync(userID);
            var nameClaim = claims.FirstOrDefault(x => x.Type == Thinktecture.IdentityServer.Core.Constants.ClaimTypes.Name);
            if (nameClaim == null) nameClaim = claims.FirstOrDefault(x => x.Type == ClaimTypes.Name);
            if (nameClaim != null) return nameClaim.Value;

            var user = await userManager.FindByIdAsync(userID);
            return user.UserName;
        }

        public virtual async Task<AuthenticateResult> AuthenticateLocalAsync(string username, string password)
        {
            if (!userManager.SupportsUserLogin)
            {
                return null;
            }

            var user = await userManager.FindByNameAsync(username);
            if (user == null)
            {
                return null;
            }

            if (userManager.SupportsUserLockout &&
                await userManager.IsLockedOutAsync(user.Id))
            {
                return null;
            }

            if (await userManager.CheckPasswordAsync(user, password))
            {
                //if (await userManager.GetTwoFactorEnabledAsync(userId) &&
                //    !await AuthenticationManager.TwoFactorBrowserRememberedAsync(user.Id))
                //{
                //    var identity = new ClaimsIdentity(DefaultAuthenticationTypes.TwoFactorCookie);
                //    identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, user.Id));
                //    AuthenticationManager.SignIn(identity);
                //    return SignInStatus.RequiresTwoFactorAuthentication;
                //}

                return new AuthenticateResult(user.Id.ToString(), await GetDisplayNameForAccountAsync(user.Id));
            }
            else if (userManager.SupportsUserLockout)
            {
                await userManager.AccessFailedAsync(user.Id);
            }

            return null;
        }

        public virtual async Task<ExternalAuthenticateResult> AuthenticateExternalAsync(string subject, ExternalIdentity externalUser)
        {
            if (externalUser == null)
            {
                throw new ArgumentNullException("externalUser");
            }

            var user = await userManager.FindAsync(new UserLoginInfo(externalUser.Provider.Name, externalUser.ProviderId));
            if (user == null)
            {
                return await ProcessNewExternalAccountAsync(externalUser.Provider.Name, externalUser.ProviderId, externalUser.Claims);
            }
            else
            {
                return await ProcessExistingExternalAccountAsync(user.Id, externalUser.Provider.Name, externalUser.ProviderId, externalUser.Claims);
            }
        }

        protected virtual async Task<ExternalAuthenticateResult> ProcessNewExternalAccountAsync(string provider, string providerId, IEnumerable<Claim> claims)
        {
            var user = new TUser { UserName = Guid.NewGuid().ToString("N") };
            var createResult = await userManager.CreateAsync(user);
            if (!createResult.Succeeded)
            {
                return new ExternalAuthenticateResult(createResult.Errors.First());
            }

            var externalLogin = new UserLoginInfo(provider, providerId);
            var addExternalResult = await userManager.AddLoginAsync(user.Id, externalLogin);
            if (!addExternalResult.Succeeded)
            {
                return new ExternalAuthenticateResult(addExternalResult.Errors.First());
            }

            var result = await AccountCreatedFromExternalProviderAsync(user.Id, provider, providerId, claims);
            if (result != null) return result;

            return await SignInFromExternalProviderAsync(user.Id, provider);
        }

        protected virtual async Task<ExternalAuthenticateResult> AccountCreatedFromExternalProviderAsync(TKey userID, string provider, string providerId, IEnumerable<Claim> claims)
        {
            claims = await SetAccountEmailAsync(userID, claims);
            claims = await SetAccountPhoneAsync(userID, claims);

            return await UpdateAccountFromExternalClaimsAsync(userID, provider, providerId, claims);
        }

        protected virtual async Task<ExternalAuthenticateResult> SignInFromExternalProviderAsync(TKey userID, string provider)
        {
            return new ExternalAuthenticateResult(provider, userID.ToString(), await GetDisplayNameForAccountAsync(userID));
        }

        protected virtual async Task<ExternalAuthenticateResult> UpdateAccountFromExternalClaimsAsync(TKey userID, string provider, string providerId, IEnumerable<Claim> claims)
        {
            var existingClaims = await userManager.GetClaimsAsync(userID);
            var intersection = existingClaims.Intersect(claims, new Thinktecture.IdentityServer.Core.Plumbing.ClaimComparer());
            var newClaims = claims.Except(intersection, new Thinktecture.IdentityServer.Core.Plumbing.ClaimComparer());

            foreach (var claim in newClaims)
            {
                var result = await userManager.AddClaimAsync(userID, claim);
                if (!result.Succeeded)
                {
                    return new ExternalAuthenticateResult(result.Errors.First());
                }
            }

            return null;
        }

        protected virtual async Task<ExternalAuthenticateResult> ProcessExistingExternalAccountAsync(TKey userID, string provider, string providerId, IEnumerable<Claim> claims)
        {
            return await SignInFromExternalProviderAsync(userID, provider);
        }

        protected virtual async Task<IEnumerable<Claim>> SetAccountEmailAsync(TKey userID, IEnumerable<Claim> claims)
        {
            var email = claims.FirstOrDefault(x => x.Type == Thinktecture.IdentityServer.Core.Constants.ClaimTypes.Email);
            if (email != null)
            {
                var userEmail = await userManager.GetEmailAsync(userID);
                if (userEmail == null)
                {
                    // if this fails, then presumably the email is already associated with another account
                    // so ignore the error and let the claim pass thru
                    var result = await userManager.SetEmailAsync(userID, email.Value);
                    if (result.Succeeded)
                    {
                        var email_verified = claims.FirstOrDefault(x => x.Type == Thinktecture.IdentityServer.Core.Constants.ClaimTypes.EmailVerified);
                        if (email_verified != null && email_verified.Value == "true")
                        {
                            var token = await userManager.GenerateEmailConfirmationTokenAsync(userID);
                            await userManager.ConfirmEmailAsync(userID, token);
                        }

                        var emailClaims = new string[] { Thinktecture.IdentityServer.Core.Constants.ClaimTypes.Email, Thinktecture.IdentityServer.Core.Constants.ClaimTypes.EmailVerified };
                        return claims.Where(x => !emailClaims.Contains(x.Type));
                    }
                }
            }

            return claims;
        }

        protected virtual async Task<IEnumerable<Claim>> SetAccountPhoneAsync(TKey userID, IEnumerable<Claim> claims)
        {
            var phone = claims.FirstOrDefault(x=>x.Type==Thinktecture.IdentityServer.Core.Constants.ClaimTypes.PhoneNumber);
            if (phone != null)
            {
                var userPhone = await userManager.GetPhoneNumberAsync(userID);
                if (userPhone == null)
                {
                    // if this fails, then presumably the phone is already associated with another account
                    // so ignore the error and let the claim pass thru
                    var result = await userManager.SetPhoneNumberAsync(userID, phone.Value);
                    if (result.Succeeded)
                    {
                        var phone_verified = claims.FirstOrDefault(x => x.Type == Thinktecture.IdentityServer.Core.Constants.ClaimTypes.PhoneNumberVerified);
                        if (phone_verified != null && phone_verified.Value == "true")
                        {
                            var token = await userManager.GenerateChangePhoneNumberTokenAsync(userID, phone.Value);
                            await userManager.ChangePhoneNumberAsync(userID, phone.Value, token);
                        }

                        var phoneClaims = new string[] { Thinktecture.IdentityServer.Core.Constants.ClaimTypes.PhoneNumber, Thinktecture.IdentityServer.Core.Constants.ClaimTypes.PhoneNumberVerified };
                        return claims.Where(x => !phoneClaims.Contains(x.Type));
                    }
                }
            }
            
            return claims;
        }

        protected virtual IEnumerable<Claim> NormalizeExternalClaimTypes(IEnumerable<Claim> incomingClaims)
        {
            return Thinktecture.IdentityServer.Core.Plumbing.ClaimMap.Map(incomingClaims);
        }

        public Task<bool> IsActive(string subject)
        {
            throw new NotImplementedException();
        }
    }
}
