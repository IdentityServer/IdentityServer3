using BrockAllen.MembershipReboot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using Thinktecture.IdentityModel.Extensions;
using Thinktecture.IdentityServer.Core;
using Thinktecture.IdentityServer.Core.Services;

namespace MembershipReboot.IdentityServer.UserService
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

        public AuthenticateResult Authenticate(string username, string password)
        {
            UserAccount acct;
            if (userAccountService.Authenticate(username, password, out acct))
            {
                return new AuthenticateResult
                {
                    Subject = acct.ID.ToString("D"),
                    Username = acct.Username
                };
            }
            
            return null;
        }

        public IEnumerable<System.Security.Claims.Claim> GetProfileData(string sub, 
            IEnumerable<string> requestedClaimTypes = null)
        {
            var acct = userAccountService.GetByID(sub.ToGuid());
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
                claims.Add(new Claim(Constants.ClaimTypes.EmailVerified, acct.IsAccountVerified?"true":"false"));
            }
            if (!String.IsNullOrWhiteSpace(acct.MobilePhoneNumber))
            {
                claims.Add(new Claim(Constants.ClaimTypes.PhoneNumber, acct.MobilePhoneNumber));
                claims.Add(new Claim(Constants.ClaimTypes.PhoneNumberVerified, !String.IsNullOrWhiteSpace(acct.MobilePhoneNumber) ? "true" : "false"));
            }
            claims.AddRange(acct.Claims.Select(x=>new Claim(x.Type, x.Value)));

            return claims.Where(x=>requestedClaimTypes.Contains(x.Type));
        }

        public AuthenticateResult Authenticate(IEnumerable<Claim> claims)
        {
            if (claims == null)
            {
                return null;
            }

            var name = claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier);
            if (name == null)
            {
                return null;
            }

            return new AuthenticateResult
            {
                Subject = name.Value,
                Username = name.Value
            };
        }
    }
}
