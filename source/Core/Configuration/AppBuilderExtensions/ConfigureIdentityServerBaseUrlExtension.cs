using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Thinktecture.IdentityServer.Core.Extensions;

namespace Owin
{
    static class ConfigureIdentityServerBaseUrlExtension
    {
        public static IAppBuilder ConfigureIdentityServerBaseUrl(this IAppBuilder app, string publicHostName)
        {
            app.Use(async (ctx, next) =>
            {
                var baseUrl = ctx.Environment.GetBaseUrl(publicHostName);
                ctx.Environment.SetIdentityServerBaseUrl(baseUrl);

                await next();
            });

            return app;
        }
    }
}
