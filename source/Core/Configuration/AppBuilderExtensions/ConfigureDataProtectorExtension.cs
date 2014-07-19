using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Thinktecture.IdentityServer.Core.Configuration;
using Microsoft.Owin.Security.DataProtection;
using Thinktecture.IdentityServer.Core;

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
