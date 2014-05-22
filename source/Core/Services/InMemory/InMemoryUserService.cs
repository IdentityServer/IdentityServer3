using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Thinktecture.IdentityServer.Core.Services.InMemory
{
    public class InMemoryUser
    {
        public string Subject { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string Provider { get; set; }
        public string ProviderId { get; set; }
        public IEnumerable<Claim> Claims { get; set; }
    }

    public class InMemoryUserService : IUserService
    {
        List<InMemoryUser> users = new List<InMemoryUser>();
        public InMemoryUserService(IEnumerable<InMemoryUser> users)
        {
            this.users.AddRange(users);
        }

        public Task<AuthenticateResult> AuthenticateLocalAsync(string username, string password)
        {
            var query =
                from u in users
                where u.Username == username && u.Password == password
                select u;
            var user = query.SingleOrDefault();
            if (user != null)
            {
                return Task.FromResult(new AuthenticateResult(user.Subject, user.Username));
            }

            return Task.FromResult<AuthenticateResult>(null);
        }

        public Task<ExternalAuthenticateResult> AuthenticateExternalAsync(string subject, Models.ExternalIdentity externalUser)
        {
            var query =
                from u in users
                where
                    u.Provider == externalUser.Provider.Name &&
                    u.ProviderId == externalUser.ProviderId
                select u;
            
            var user = query.SingleOrDefault();
            if (user == null)
            {
                var name = externalUser.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Name);
                if (name == null)
                {
                    return Task.FromResult<ExternalAuthenticateResult>(null);
                }
                var claims = externalUser.Claims.Except(new Claim[] { name });

                user = new InMemoryUser
                {
                    Subject = Guid.NewGuid().ToString("N"),
                    Provider = externalUser.Provider.Name,
                    ProviderId = externalUser.ProviderId,
                    Username = name.Value,
                    Claims = claims.ToArray()
                };
            }

            return Task.FromResult(new ExternalAuthenticateResult(user.ProviderId, user.Subject, user.Username));
        }


        public Task<IEnumerable<System.Security.Claims.Claim>> GetProfileDataAsync(string subject, IEnumerable<string> requestedClaimTypes = null)
        {
            var query =
                from u in users
                where u.Subject == subject
                select u;
            var user = query.Single();

            var claims = user.Claims;
            if (requestedClaimTypes != null)
            {
                claims = claims.Where(x => requestedClaimTypes.Contains(x.Type));
            }

            return Task.FromResult<IEnumerable<Claim>>(claims);
        }
    }
}
