using System.Collections.Generic;
using System.Security.Claims;

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

            foreach (var claim in claims)
            {
                if (!d.ContainsKey(claim.Type))
                {
                    d.Add(claim.Type, claim.Value);
                }
                else
                {
                    var value = d[claim.Type];

                    var list = value as HashSet<object>;
                    if (list != null)
                    {
                        list.Add(claim.Value);
                    }
                    else
                    {
                        d.Remove(claim.Type);
                        d.Add(claim.Type, new HashSet<object> { value, claim.Value });
                    }
                }
            }

            return d;
        }
    }
}
