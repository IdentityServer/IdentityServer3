using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Thinktecture.IdentityServer.Core.Services.InMemory
{
    public class InMemoryConsentService : IConsentService
    {
        public Task<bool> RequiresConsentAsync(Models.Client client, System.Security.Claims.ClaimsPrincipal user, IEnumerable<string> scopes)
        {
            return Task.FromResult(true);
        }

        public Task UpdateConsentAsync(Models.Client client, System.Security.Claims.ClaimsPrincipal user, IEnumerable<string> scopes)
        {
            return Task.FromResult(0);
        }
    }
}
