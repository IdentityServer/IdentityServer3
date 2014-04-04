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
using Thinktecture.IdentityServer.Core;
using Thinktecture.IdentityServer.Core.Services;

namespace Thinktecture.IdentityServer.AspNetIdentity
{
    public class UserService<TUser> : IUserService, IDisposable
        where TUser : class, IUser, new()
    {
        protected readonly UserManager<TUser> userManager;
        IDisposable cleanup;
        public UserService(UserManager<TUser> userManager, IDisposable cleanup = null)
        {
            if (userManager == null) throw new ArgumentNullException("userManager");

            this.userManager = userManager;
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
            var acct = await userManager.FindByIdAsync(subject);
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
                new Claim(Thinktecture.IdentityServer.Core.Constants.ClaimTypes.Subject, user.Id),
                new Claim(Thinktecture.IdentityServer.Core.Constants.ClaimTypes.Name, user.UserName),
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

                return new AuthenticateResult(user.Id, username);
            }
            else if (userManager.SupportsUserLockout)
            {
                await userManager.AccessFailedAsync(user.Id);
            }

            return null;
        }

        public virtual async Task<ExternalAuthenticateResult> AuthenticateExternalAsync(string subject, IEnumerable<Claim> externalClaims)
        {
            if (externalClaims == null || !externalClaims.Any())
            {
                return null;
            }

            var subClaim = externalClaims.FirstOrDefault(x => x.Type == Thinktecture.IdentityServer.Core.Constants.ClaimTypes.Subject);
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

            var user = await userManager.FindAsync(new UserLoginInfo(provider, providerId));
            if (user == null)
            {
                return await ProcessNewExternalAccountAsync(provider, providerId, externalClaims);
            }
            else
            {
                return await ProcessExistingExternalAccountAsync(user.Id, provider, providerId, externalClaims);
            }
        }

        protected virtual async Task<ExternalAuthenticateResult> ProcessNewExternalAccountAsync(string provider, string providerId, IEnumerable<Claim> claims)
        {
            var createResult = await CreateNewUserFromExternalAccountAsync(provider, providerId, claims);
            if (createResult.Item1 != null)
            {
                return createResult.Item1;
            }

            var userID = createResult.Item2;
            var result = await AccountCreatedFromExternalProviderAsync(userID, provider, providerId, claims);
            if (result != null) return result;

            return await SignInFromExternalProviderAsync(userID, provider);
        }

        protected virtual async Task<Tuple<ExternalAuthenticateResult, string>> CreateNewUserFromExternalAccountAsync(string provider, string providerId, IEnumerable<Claim> claims)
        {
            string userID = null;
            var name = GetNameFromClaims(claims);
            if (name != null)
            {
                // guess at a username from external provider's supplied name
                var user = await userManager.FindByNameAsync(name);
                if (user == null)
                {
                    user = new TUser { UserName = name };
                    var createResult = await userManager.CreateAsync(user);
                    if (createResult.Succeeded)
                    {
                        userID = user.Id;
                    }
                }
            }

            if (userID == null)
            {
                // generate a username
                var user = new TUser { UserName = Guid.NewGuid().ToString("N") };
                var createResult = await userManager.CreateAsync(user);
                if (!createResult.Succeeded)
                {
                    return new Tuple<ExternalAuthenticateResult, string>(new ExternalAuthenticateResult(createResult.Errors.First()), null);
                }
                userID = user.Id;
            }

            return new Tuple<ExternalAuthenticateResult, string>(null, userID);
        }

        protected virtual async Task<ExternalAuthenticateResult> AccountCreatedFromExternalProviderAsync(string userID, string provider, string providerId, IEnumerable<Claim> claims)
        {
            var externalLogin = new UserLoginInfo(provider, providerId);
            var addExternalResult = await userManager.AddLoginAsync(userID, externalLogin);
            if (!addExternalResult.Succeeded)
            {
                return new ExternalAuthenticateResult(addExternalResult.Errors.First());
            }
            
            claims = FilterExternalClaimsForNewAccount(provider, claims);

            var result = await SetNewAccountEmailAsync(userID, provider, claims);
            if (result.Item1 != null) return result.Item1;
            claims = result.Item2;

            result = await SetNewAccountPhoneAsync(userID, provider, claims);
            if (result.Item1 != null) return result.Item1;
            claims = result.Item2;

            return await UpdateAccountFromExternalClaimsAsync(userID, provider, providerId, claims);
        }

        protected virtual async Task<ExternalAuthenticateResult> SignInFromExternalProviderAsync(string userID, string provider)
        {
            return new ExternalAuthenticateResult(provider, userID, await GetNameForAccount(userID));
        }

        protected virtual async Task<ExternalAuthenticateResult> ProcessExistingExternalAccountAsync(string userID, string provider, string providerId, IEnumerable<Claim> claims)
        {
            claims = FilterExternalClaimsForExistingAccount(userID, provider, claims);

            claims = await SetAccountEmailAsync(userID, claims);
            claims = await SetAccountPhoneAsync(userID, claims);

            var result = await UpdateAccountFromExternalClaimsAsync(userID, provider, providerId, claims);
            if (result != null) return result;

            return await SignInFromExternalProviderAsync(userID, provider);
        }

        protected virtual async Task<ExternalAuthenticateResult> UpdateAccountFromExternalClaimsAsync(string userID, string provider, string providerId, IEnumerable<Claim> claims)
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

        protected virtual IEnumerable<Claim> FilterExternalClaimsForNewAccount(string provider, IEnumerable<Claim> claims)
        {
            return claims;
        }

        protected virtual IEnumerable<Claim> FilterExternalClaimsForExistingAccount(string userID, string provider, IEnumerable<Claim> claims)
        {
            return claims;
        }

        protected virtual async Task<Tuple<ExternalAuthenticateResult, IEnumerable<Claim>>> SetNewAccountEmailAsync(string userID, string provider, IEnumerable<Claim> claims)
        {
            var result = await SetAccountEmailAsync(userID, claims);
            return new Tuple<ExternalAuthenticateResult,IEnumerable<Claim>>(null, result);
        }

        protected virtual async Task<IEnumerable<Claim>> SetAccountEmailAsync(string userID, IEnumerable<Claim> claims)
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

        protected virtual async Task<Tuple<ExternalAuthenticateResult, IEnumerable<Claim>>> SetNewAccountPhoneAsync(string userID, string provider, IEnumerable<Claim> claims)
        {
            var result = await SetAccountPhoneAsync(userID, claims);
            return new Tuple<ExternalAuthenticateResult, IEnumerable<Claim>>(null, result);
        }

        protected virtual async Task<IEnumerable<Claim>> SetAccountPhoneAsync(string userID, IEnumerable<Claim> claims)
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

        protected virtual async Task<string> GetNameForAccount(string userID)
        {
            var user = await userManager.FindByIdAsync(userID);
            string name = null;

            // if the user has a local password, then presumably they know
            // their own username and will recognize it (rather than a generated one)
            if (await userManager.HasPasswordAsync(userID)) name = user.UserName;

            if (name == null)
            {
                var claims = await userManager.GetClaimsAsync(userID);
                name = GetNameFromClaims(claims);
            }

            name = name ?? user.UserName;
            return name;
        }

        protected virtual string GetNameFromClaims(IEnumerable<Claim> claims)
        {
            return claims.Where(x =>
                    x.Type == Thinktecture.IdentityServer.Core.Constants.ClaimTypes.PreferredUserName ||
                    x.Type == Thinktecture.IdentityServer.Core.Constants.ClaimTypes.Name ||
                    x.Type == ClaimTypes.Name)
                .Select(x => x.Value)
                .FirstOrDefault();
        }

        protected virtual IEnumerable<Claim> NormalizeExternalClaimTypes(IEnumerable<Claim> incomingClaims)
        {
            return Thinktecture.IdentityServer.Core.Plumbing.ClaimMap.Map(incomingClaims);
        }
    }
}
