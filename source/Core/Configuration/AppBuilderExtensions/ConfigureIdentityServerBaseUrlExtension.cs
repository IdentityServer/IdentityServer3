/*
 * Copyright (c) Dominick Baier, Brock Allen.  All rights reserved.
 * see license
 */

using System;
using Microsoft.Owin;
using Thinktecture.IdentityServer.Core.Extensions;

namespace Owin
{
    static class ConfigureIdentityServerBaseUrlExtension
    {
        public static IAppBuilder ConfigureIdentityServerBaseUrl(this IAppBuilder app, Func<IOwinContext, string> getPublicHostName)
        {
            app.Use(async (ctx, next) =>
            {
                var baseUrl = ctx.Environment.GetBaseUrl(getPublicHostName);
                ctx.Environment.SetIdentityServerBaseUrl(baseUrl);

                await next();
            });

            return app;
        }
    }
}
