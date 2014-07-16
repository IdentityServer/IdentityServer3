/*
 * Copyright (c) Dominick Baier, Brock Allen.  All rights reserved.
 * see license
 */

using Microsoft.Owin;
using Microsoft.Owin.Extensions;
using Microsoft.Owin.FileSystems;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.DataProtection;
using Microsoft.Owin.StaticFiles;
using System;
using System.IdentityModel.Tokens;
using Thinktecture.IdentityModel.Tokens;
using Thinktecture.IdentityServer.Core;
using Thinktecture.IdentityServer.Core.Configuration;
using Thinktecture.IdentityServer.Core.Hosting;

namespace Owin
{
    public static class UseHstsExtension
    {
        public static IAppBuilder UseHsts(this IAppBuilder app, TimeSpan duration)
        {
            if (app == null) throw new ArgumentNullException("app");
            if (duration < TimeSpan.Zero) throw new ArgumentException("duration cannot be below zero");

            if (duration > TimeSpan.Zero)
            {
                int seconds = (int)duration.TotalSeconds;
                app.Use(async (ctx, next) =>
                {
                    if (ctx.Request.IsSecure)
                    {
                        ctx.Response.Headers.Append("Strict-Transport-Security", "max-age:" + seconds);
                    }
                    await next();
                });
            }

            return app;
        }

        public static IAppBuilder UseHsts(this IAppBuilder app, int days = 30)
        {
            if (days < 0) throw new ArgumentException("days cannot be below zero");

            return app.UseHsts(TimeSpan.FromDays(days));
        }
    }
}