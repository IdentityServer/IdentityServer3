using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using Thinktecture.IdentityServer.Core.Plumbing;

namespace Thinktecture.IdentityServer.Core.Extensions
{
    public static class ClaimListExtensions
    {
        public static Dictionary<string, object> ToClaimsDictionary(this IEnumerable<Claim> claims)
        {
            var d = new Dictionary<string, object>();

            if (claims == null)
            {
                return d;
            }

            var distinctClaims = claims.Distinct(new ClaimComparer());

            foreach (var claim in distinctClaims)
            {
                if (!d.ContainsKey(claim.Type))
                {
                    d.Add(claim.Type, claim.Value);
                }
                else
                {
                    var value = d[claim.Type];

                    var list = value as List<object>;
                    if (list != null)
                    {
                        list.Add(claim.Value);
                    }
                    else
                    {
                        d.Remove(claim.Type);
                        d.Add(claim.Type, new List<object> { value, claim.Value });
                    }
                }
            }

            return d;
        }
    }
}
