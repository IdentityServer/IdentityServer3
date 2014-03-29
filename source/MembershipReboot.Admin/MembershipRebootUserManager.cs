using BrockAllen.MembershipReboot;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Thinktecture.IdentityServer.Admin.Core;
using Thinktecture.IdentityServer.Core;

namespace MembershipReboot.IdentityServer.Admin
{
    public class MembershipRebootUserManager : IUserManager, IDisposable
    {
        UserAccountService userAccountService;
        IUserAccountQuery query;
        IDisposable cleanup;

        public MembershipRebootUserManager(
            UserAccountService userAccountService,
            IUserAccountQuery query,
            IDisposable cleanup)
        {
            if (userAccountService == null) throw new ArgumentNullException("userAccountService");
            if (query == null) throw new ArgumentNullException("query");

            this.userAccountService = userAccountService;
            this.query = query;
            this.cleanup = cleanup;
        }

        public void Dispose()
        {
            if (this.cleanup != null)
            {
                cleanup.Dispose();
                cleanup = null;
            }
        }

        public Task<UserManagerMetadata> GetMetadataAsync()
        {
            var claims = new ClaimMetadata[]
            {
                new ClaimMetadata{
                    ClaimType = Constants.ClaimTypes.Subject,
                    DisplayName = "Subject",
                }
            };

            return Task.FromResult(new UserManagerMetadata
            {
                UniqueIdentitiferClaimType = Constants.ClaimTypes.Subject,
                Claims = claims
            });
        }

        public Task<UserManagerResult<QueryResult>> QueryUsersAsync(string filter, int start, int count)
        {
            int total;
            var users = query.Query(filter, start, count, out total);

            var result = new QueryResult();
            result.Start = start;
            result.Count = count;
            result.Total = total;
            result.Filter = filter;
            result.Users = users.Select(x =>
            {
                var user = new UserResult
                {
                    Subject = x.ID.ToString("D"),
                    Username = x.Username
                };
                
                return user;
            }).ToArray();

            return Task.FromResult(new UserManagerResult<QueryResult>(result));
        }

        public async Task<UserManagerResult<UserResult>> GetUserAsync(string subject)
        {
            Guid g;
            if (!Guid.TryParse(subject, out g))
            {
                return new UserManagerResult<UserResult>("Invalid user.");
            }

            try
            {
                var acct = this.userAccountService.GetByID(g);
                if (acct == null)
                {
                    return new UserManagerResult<UserResult>("Invalid user.");
                }

                var user = new UserResult
                {
                    Subject = subject,
                    Username = acct.Username
                };
                var claims = new List<Thinktecture.IdentityServer.Admin.Core.UserClaim>();
                if (!String.IsNullOrWhiteSpace(acct.Email) && !this.userAccountService.Configuration.EmailIsUsername)
                {
                    claims.Add(new Thinktecture.IdentityServer.Admin.Core.UserClaim { Type = Constants.ClaimTypes.Email, Value = acct.Email });
                    claims.Add(new Thinktecture.IdentityServer.Admin.Core.UserClaim { Type = Constants.ClaimTypes.EmailVerified, Value = acct.IsAccountVerified ? "true" : "false" });
                }
                if (!String.IsNullOrWhiteSpace(acct.MobilePhoneNumber))
                {
                    claims.Add(new Thinktecture.IdentityServer.Admin.Core.UserClaim { Type = Constants.ClaimTypes.PhoneNumber, Value = acct.MobilePhoneNumber });
                    claims.Add(new Thinktecture.IdentityServer.Admin.Core.UserClaim { Type = Constants.ClaimTypes.PhoneNumberVerified, Value = !String.IsNullOrWhiteSpace(acct.MobilePhoneNumber) ? "true" : "false" });
                }
                if (acct.Claims != null)
                {
                    claims.AddRange(acct.Claims.Select(x => new Thinktecture.IdentityServer.Admin.Core.UserClaim { Type = x.Type, Value = x.Value }));
                }
                user.Claims = claims.ToArray();
                
                return new UserManagerResult<UserResult>(user);
            }
            catch (ValidationException ex)
            {
                return new UserManagerResult<UserResult>(ex.Message);
            }
        }

        public Task<UserManagerResult<CreateResult>> CreateUserAsync(string username, string password)
        {
            try
            {
                UserAccount acct;
                if (this.userAccountService.Configuration.EmailIsUsername)
                {
                    acct = this.userAccountService.CreateAccount(null, password, username);
                }
                else
                {
                    acct = this.userAccountService.CreateAccount(username, password, null);
                }

                return Task.FromResult(new UserManagerResult<CreateResult>(new CreateResult { Subject = acct.ID.ToString("D") }));
            }
            catch (ValidationException ex)
            {
                return Task.FromResult(new UserManagerResult<CreateResult>(ex.Message));
            }
        }
        
        public Task<UserManagerResult> DeleteUserAsync(string subject)
        {
            Guid g;
            if (!Guid.TryParse(subject, out g))
            {
                return Task.FromResult(new UserManagerResult("Invalid user."));
            }

            try
            {
                this.userAccountService.DeleteAccount(g);
            }
            catch (ValidationException ex)
            {
                return Task.FromResult(new UserManagerResult(ex.Message));
            } 

            return Task.FromResult(new UserManagerResult());
        }

        public async Task<UserManagerResult> SetPasswordAsync(string id, string password)
        {
            Guid g;
            if (!Guid.TryParse(id, out g))
            {
                return new UserManagerResult("Invalid id");
            }

            try
            {
                this.userAccountService.SetPassword(g, password);
            }
            catch (ValidationException ex)
            {
                return new UserManagerResult(ex.Message);
            }

            return UserManagerResult.Success;
        }
        
        public Task<UserManagerResult> AddClaimAsync(string subject, string type, string value)
        {
            Guid g;
            if (!Guid.TryParse(subject, out g))
            {
                return Task.FromResult(new UserManagerResult("Invalid user."));
            }

            try
            {
                this.userAccountService.AddClaim(g, type, value);
            }
            catch (ValidationException ex)
            {
                return Task.FromResult(new UserManagerResult(ex.Message));
            } 

            return Task.FromResult(new UserManagerResult());
        }

        public Task<UserManagerResult> DeleteClaimAsync(string subject, string type, string value)
        {
            Guid g;
            if (!Guid.TryParse(subject, out g))
            {
                return Task.FromResult(new UserManagerResult("Invalid user."));
            }

            try
            {
                this.userAccountService.RemoveClaim(g, type, value);
            }
            catch (ValidationException ex)
            {
                return Task.FromResult(new UserManagerResult(ex.Message));
            } 

            return Task.FromResult(new UserManagerResult());
        }
    }
}
