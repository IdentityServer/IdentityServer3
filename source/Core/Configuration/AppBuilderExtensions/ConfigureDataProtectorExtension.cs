/*
 * Copyright (c) Dominick Baier, Brock Allen.  All rights reserved.
 * see license
 */
using Microsoft.Owin.Security.DataProtection;
using Thinktecture.IdentityServer.Core;
using Thinktecture.IdentityServer.Core.Configuration;

namespace Owin
{
    static class ConfigureDataProtectorExtension
    {
        public static IAppBuilder ConfigureDataProtectionProvider(this IAppBuilder app, IdentityServerOptions options)
        {
            if (options.DataProtector == null)
            {
                var provider = app.GetDataProtectionProvider();
                if (provider == null)
                {
                    provider = new DpapiDataProtectionProvider(Constants.PrimaryAuthenticationType);
                }

                options.DataProtector = new HostDataProtector(provider);
            } 
            return app;
        }
    }
}
