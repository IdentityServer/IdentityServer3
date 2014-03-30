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

        public async Task<IEnumerable<System.Security.Claims.Claim>> GetProfileDataAsync(string subject,
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
            claims.AddRange(account.LinkedAccountClaims.Select(x => new Claim(x.Type, x.Value)));

            return claims;
        }
        
        public async Task<AuthenticateResult> AuthenticateLocalAsync(string username, string password)
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

        public async Task<ExternalAuthenticateResult> AuthenticateExternalAsync(string subject, IEnumerable<Claim> externalClaims)
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
                    return await ProcessExistingExternalAccountAsync(provider, providerId, acct, externalClaims);
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
            
            var result = await AccountCreatedFromExternalProviderAsync(provider, providerId, acct.ID, claims);
            if (result != null) return result;
            
            return await SignInFromExternalProviderAsync(provider, acct.ID);
        }

        protected virtual async Task<ExternalAuthenticateResult> AccountCreatedFromExternalProviderAsync(string provider, string providerId, Guid accountID, IEnumerable<Claim> claims)
        {
            claims = FilterExternalClaimsForNewAccount(provider, claims);

            return await UpdateAccountFromExternalClaimsAsync(provider, providerId, accountID, claims);
        }

        protected async Task<ExternalAuthenticateResult> SignInFromExternalProviderAsync(string provider, Guid accountID)
        {
            var acct = userAccountService.GetByID(accountID);
            return new ExternalAuthenticateResult(provider, acct.ID.ToString(), GetNameForAccount(acct));
        }

        private async Task<ExternalAuthenticateResult> ProcessExistingExternalAccountAsync(string provider, string providerId, UserAccount acct, IEnumerable<Claim> claims)
        {
            claims = FilterExternalClaimsForExistingAccount(provider, claims);

            var result = await UpdateAccountFromExternalClaimsAsync(provider, providerId, acct.ID, claims);
            if (result != null) return result;

            return await SignInFromExternalProviderAsync(provider, acct.ID);
        }

        protected virtual async Task<ExternalAuthenticateResult> UpdateAccountFromExternalClaimsAsync(string provider, string providerId, Guid accountID, IEnumerable<Claim> claims)
        {
            var account = userAccountService.GetByID(accountID);
            userAccountService.AddOrUpdateLinkedAccount(account, provider, providerId, claims);

            return null;
        }
        
        protected virtual IEnumerable<Claim> FilterExternalClaimsForNewAccount(string provider, IEnumerable<Claim> claims)
        {
            return claims;
        }

        protected virtual IEnumerable<Claim> FilterExternalClaimsForExistingAccount(string provider, IEnumerable<Claim> claims)
        {
            return claims;
        }

        static string[] EmailAndPhoneClaims = new[] {
            Constants.ClaimTypes.Email,
            Constants.ClaimTypes.EmailVerified,
            Constants.ClaimTypes.PhoneNumber,
            Constants.ClaimTypes.PhoneNumberVerified,
        };

        protected virtual void UpdateAccountFromClaims(Guid accountID, IEnumerable<Claim> claims)
        {
            var account = userAccountService.GetByID(accountID);

            UpdateEmail(account, claims);
            UpdatePhone(account, claims);

            claims = claims.Where(x => !EmailAndPhoneClaims.Contains(x.Type));

            var oldClaims = account.Claims.Select(x => new Tuple<string, string>(x.Type, x.Value));
            var newClaims = claims.Select(x => new Tuple<string, string>(x.Type, x.Value));
            
            var intersection = newClaims.Intersect(oldClaims);
            var claimsToAdd = newClaims.Except(intersection).Select(x=>new UserClaim(x.Item1, x.Item2));
            var claimsToRemove = oldClaims.Except(intersection).Select(x => new UserClaim(x.Item1, x.Item2));

            userAccountService.UpdateClaims(account.ID, claimsToAdd.ToCollection(), claimsToRemove.ToCollection());
        }

        protected virtual void UpdateEmail(UserAccount acct, IEnumerable<Claim> claims)
        {
            var email = ClaimHelper.GetValue(claims, Constants.ClaimTypes.Email);
            if (email != null && email != acct.Email)
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
            }
        }
        
        protected virtual void UpdatePhone(UserAccount acct, IEnumerable<Claim> claims)
        {
            var phone = ClaimHelper.GetValue(claims, Constants.ClaimTypes.PhoneNumber);
            if (phone != null && acct.MobilePhoneNumber != phone)
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
            }
        }

        protected virtual string GetNameForAccount(UserAccount acct)
        {
            string name = null;
            
            // if the user has a local password, then presumably they know
            // their own username and will recognize it (rather than a generated one)
            if (acct.HasPassword()) name = acct.Username;

            if (name == null) name = acct.GetClaimValue(Constants.ClaimTypes.PreferredUserName);
            if (name == null) name = acct.GetClaimValue(Constants.ClaimTypes.Name);
            if (name == null) name = acct.GetClaimValue(ClaimTypes.Name);

            name = name ?? acct.Username;
            return name;
        }

        static Dictionary<string, string> ClaimTypeMap = BuildClaimTypeMap();

        static Dictionary<string, string> BuildClaimTypeMap()
        {
            Dictionary<string, string> dictionary = new Dictionary<string, string>();
            dictionary.Add("actort", "http://schemas.xmlsoap.org/ws/2009/09/identity/claims/actor");
            dictionary.Add("birthdate", "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/dateofbirth");
            dictionary.Add("email", "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress");
            dictionary.Add("family_name", "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/surname");
            dictionary.Add("gender", "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/gender");
            dictionary.Add("given_name", "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/givenname");
            dictionary.Add("sub", "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier");
            dictionary.Add("nameid", "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier");
            dictionary.Add("website", "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/webpage");
            dictionary.Add("unique_name", "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name");
            dictionary.Add("oid", "http://schemas.microsoft.com/identity/claims/objectidentifier");
            dictionary.Add("scp", "http://schemas.microsoft.com/identity/claims/scope");
            dictionary.Add("tid", "http://schemas.microsoft.com/identity/claims/tenantid");
            dictionary.Add("acr", "http://schemas.microsoft.com/claims/authnclassreference");
            dictionary.Add("adfs1email", "http://schemas.xmlsoap.org/claims/EmailAddress");
            dictionary.Add("adfs1upn", "http://schemas.xmlsoap.org/claims/UPN");
            dictionary.Add("amr", "http://schemas.microsoft.com/claims/authnmethodsreferences");
            dictionary.Add("auth_time", "http://schemas.microsoft.com/ws/2008/06/identity/claims/authenticationinstant");
            dictionary.Add("authmethod", "http://schemas.microsoft.com/ws/2008/06/identity/claims/authenticationmethod");
            dictionary.Add("certapppolicy", "http://schemas.microsoft.com/2012/12/certificatecontext/extension/applicationpolicy");
            dictionary.Add("certauthoritykeyidentifier", "http://schemas.microsoft.com/2012/12/certificatecontext/extension/authoritykeyidentifier");
            dictionary.Add("certbasicconstraints", "http://schemas.microsoft.com/2012/12/certificatecontext/extension/basicconstraints");
            dictionary.Add("certeku", "http://schemas.microsoft.com/2012/12/certificatecontext/extension/eku");
            dictionary.Add("certissuer", "http://schemas.microsoft.com/2012/12/certificatecontext/field/issuer");
            dictionary.Add("certissuername", "http://schemas.microsoft.com/2012/12/certificatecontext/field/issuername");
            dictionary.Add("certkeyusage", "http://schemas.microsoft.com/2012/12/certificatecontext/extension/keyusage");
            dictionary.Add("certnotafter", "http://schemas.microsoft.com/2012/12/certificatecontext/field/notafter");
            dictionary.Add("certnotbefore", "http://schemas.microsoft.com/2012/12/certificatecontext/field/notbefore");
            dictionary.Add("certpolicy", "http://schemas.microsoft.com/2012/12/certificatecontext/extension/certificatepolicy");
            dictionary.Add("certpublickey", "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/rsa");
            dictionary.Add("certrawdata", "http://schemas.microsoft.com/2012/12/certificatecontext/field/rawdata");
            dictionary.Add("certserialnumber", "http://schemas.microsoft.com/ws/2008/06/identity/claims/serialnumber");
            dictionary.Add("certsignaturealgorithm", "http://schemas.microsoft.com/2012/12/certificatecontext/field/signaturealgorithm");
            dictionary.Add("certsubject", "http://schemas.microsoft.com/2012/12/certificatecontext/field/subject");
            dictionary.Add("certsubjectaltname", "http://schemas.microsoft.com/2012/12/certificatecontext/extension/san");
            dictionary.Add("certsubjectkeyidentifier", "http://schemas.microsoft.com/2012/12/certificatecontext/extension/subjectkeyidentifier");
            dictionary.Add("certsubjectname", "http://schemas.microsoft.com/2012/12/certificatecontext/field/subjectname");
            dictionary.Add("certtemplateinformation", "http://schemas.microsoft.com/2012/12/certificatecontext/extension/certificatetemplateinformation");
            dictionary.Add("certtemplatename", "http://schemas.microsoft.com/2012/12/certificatecontext/extension/certificatetemplatename");
            dictionary.Add("certthumbprint", "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/thumbprint");
            dictionary.Add("certx509version", "http://schemas.microsoft.com/2012/12/certificatecontext/field/x509version");
            dictionary.Add("clientapplication", "http://schemas.microsoft.com/2012/01/requestcontext/claims/x-ms-client-application");
            dictionary.Add("clientip", "http://schemas.microsoft.com/2012/01/requestcontext/claims/x-ms-client-ip");
            dictionary.Add("clientuseragent", "http://schemas.microsoft.com/2012/01/requestcontext/claims/x-ms-client-user-agent");
            dictionary.Add("commonname", "http://schemas.xmlsoap.org/claims/CommonName");
            dictionary.Add("denyonlyprimarygroupsid", "http://schemas.microsoft.com/ws/2008/06/identity/claims/denyonlyprimarygroupsid");
            dictionary.Add("denyonlyprimarysid", "http://schemas.microsoft.com/ws/2008/06/identity/claims/denyonlyprimarysid");
            dictionary.Add("denyonlysid", "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/denyonlysid");
            dictionary.Add("devicedispname", "http://schemas.microsoft.com/2012/01/devicecontext/claims/displayname");
            dictionary.Add("deviceid", "http://schemas.microsoft.com/2012/01/devicecontext/claims/identifier");
            dictionary.Add("deviceismanaged", "http://schemas.microsoft.com/2012/01/devicecontext/claims/ismanaged");
            dictionary.Add("deviceostype", "http://schemas.microsoft.com/2012/01/devicecontext/claims/ostype");
            dictionary.Add("deviceosver", "http://schemas.microsoft.com/2012/01/devicecontext/claims/osversion");
            dictionary.Add("deviceowner", "http://schemas.microsoft.com/2012/01/devicecontext/claims/userowner");
            dictionary.Add("deviceregid", "http://schemas.microsoft.com/2012/01/devicecontext/claims/registrationid");
            dictionary.Add("endpointpath", "http://schemas.microsoft.com/2012/01/requestcontext/claims/x-ms-endpoint-absolute-path");
            dictionary.Add("forwardedclientip", "http://schemas.microsoft.com/2012/01/requestcontext/claims/x-ms-forwarded-client-ip");
            dictionary.Add("group", "http://schemas.xmlsoap.org/claims/Group");
            dictionary.Add("groupsid", "http://schemas.microsoft.com/ws/2008/06/identity/claims/groupsid");
            dictionary.Add("idp", "http://schemas.microsoft.com/identity/claims/identityprovider");
            dictionary.Add("insidecorporatenetwork", "http://schemas.microsoft.com/ws/2012/01/insidecorporatenetwork");
            dictionary.Add("isregistereduser", "http://schemas.microsoft.com/2012/01/devicecontext/claims/isregistereduser");
            //dictionary.Add("ppid", ClaimTypes.PPID);
            dictionary.Add("primarygroupsid", "http://schemas.microsoft.com/ws/2008/06/identity/claims/primarygroupsid");
            dictionary.Add("primarysid", "http://schemas.microsoft.com/ws/2008/06/identity/claims/primarysid");
            dictionary.Add("proxy", "http://schemas.microsoft.com/2012/01/requestcontext/claims/x-ms-proxy");
            dictionary.Add("pwdchgurl", "http://schemas.microsoft.com/ws/2012/01/passwordchangeurl");
            dictionary.Add("pwdexpdays", "http://schemas.microsoft.com/ws/2012/01/passwordexpirationdays");
            dictionary.Add("pwdexptime", "http://schemas.microsoft.com/ws/2012/01/passwordexpirationtime");
            dictionary.Add("relyingpartytrustid", "http://schemas.microsoft.com/2012/01/requestcontext/claims/relyingpartytrustid");
            dictionary.Add("role", "http://schemas.microsoft.com/ws/2008/06/identity/claims/role");
            dictionary.Add("upn", "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/upn");
            dictionary.Add("winaccountname", "http://schemas.microsoft.com/ws/2008/06/identity/claims/windowsaccountname");

            Dictionary<string, string> dictionary2 = new Dictionary<string, string>();
            foreach(var item in dictionary)
            {
                if (!dictionary2.ContainsKey(item.Value))
                {
                    dictionary2.Add(item.Value, item.Key);
                }
            }

            return dictionary2;
        }

        static string MapClaimType(string type)
        {
            if (ClaimTypeMap.ContainsKey(type))
            { 
                return ClaimTypeMap[type];
            }
            
            return type;
        }

        protected virtual IEnumerable<Claim> NormalizeExternalClaimTypes(IEnumerable<Claim> incomingClaims)
        {
            var claimsToMap = incomingClaims.Where(x => ClaimTypeMap.ContainsKey(x.Type));
            var mappedClaims = claimsToMap.Select(x => new Claim(MapClaimType(x.Type), x.Value));

            var claims = new List<Claim>();
            claims.AddRange(incomingClaims.Except(claimsToMap));
            claims.AddRange(mappedClaims);
            
            return claims;
        }
    }
}
