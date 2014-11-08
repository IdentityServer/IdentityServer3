using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Thinktecture.IdentityServer.Core.Services.Default
{
    public class IgnoreClaimsFilter : IExternalClaimsFilter
    {
        readonly string[] claimTypesToIgnore;

        public IgnoreClaimsFilter(params string[] claimTypesToIgnore)
        {
            this.claimTypesToIgnore = claimTypesToIgnore;
        }
        
        public IEnumerable<Claim> Filter(string provider, IEnumerable<Claim> claims)
        {
            return claims.Where(x => !claimTypesToIgnore.Contains(x.Type));
        }
    }
}
