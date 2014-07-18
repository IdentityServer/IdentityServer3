using Microsoft.Owin.Cors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Thinktecture.IdentityServer.Core.Configuration
{
    class CorsPolicyProvider : ICorsPolicyProvider
    {
        CorsPolicy policy;
        public CorsPolicyProvider(CorsPolicy policy)
        {
            if (policy == null) throw new ArgumentNullException("policy");
            this.policy = policy;
        }

        public async Task<System.Web.Cors.CorsPolicy> GetCorsPolicyAsync(Microsoft.Owin.IOwinRequest request)
        {
            var origin = request.Headers["Origin"];
            if (origin != null)
            {
                if (policy.AllowedOrigins.Contains(origin))
                {
                    return Allow(origin);
                }

                if (policy.PolicyCallback != null)
                {
                    if (await policy.PolicyCallback(origin))
                    {
                        return Allow(origin);
                    }
                }
            }
            return null;
        }

        System.Web.Cors.CorsPolicy Allow(string origin)
        {
            var policy = new System.Web.Cors.CorsPolicy
            {
                AllowAnyHeader = true,
                AllowAnyMethod = true,
            };
            policy.Origins.Add(origin);
            return policy;
        }
    }
}
