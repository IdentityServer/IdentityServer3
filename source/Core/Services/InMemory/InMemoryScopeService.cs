using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Thinktecture.IdentityServer.Core.Services.InMemory
{
    public class InMemoryScopeService : IScopeService
    {
        IEnumerable<Models.Scope> scopes;
        public InMemoryScopeService(IEnumerable<Models.Scope> scopes)
        {
            this.scopes = scopes;
        }

        public Task<IEnumerable<Models.Scope>> GetScopesAsync()
        {
            return Task.FromResult(scopes);
        }
    }
}
