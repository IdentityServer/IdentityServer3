/*
 * Copyright (c) Dominick Baier, Brock Allen.  All rights reserved.
 * see license
 */
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
