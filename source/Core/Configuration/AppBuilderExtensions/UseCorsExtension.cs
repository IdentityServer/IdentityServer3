using Owin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Thinktecture.IdentityServer.Core;
using Thinktecture.IdentityServer.Core.Configuration;

namespace Owin
{
    static class UseCorsExtension
    {
        public static void UseCors(this IAppBuilder app, CorsPolicy policy)
        {
            if (policy != null)
            {
                app.UseCors(new Microsoft.Owin.Cors.CorsOptions
                {
                    PolicyProvider = new CorsPolicyProvider(policy, Constants.RoutePaths.CorsPaths)
                });
            }
        }
    }
}
