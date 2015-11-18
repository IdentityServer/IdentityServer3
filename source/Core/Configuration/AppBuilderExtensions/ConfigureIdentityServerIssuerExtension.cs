/*
 * Copyright 2014, 2015 Dominick Baier, Brock Allen
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *   http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using IdentityServer3.Core.Configuration;
using IdentityServer3.Core.Extensions;
using Microsoft.Owin;
using System;

namespace Owin
{
    internal static class ConfigureIdentityServerIssuerExtension
    {
        // todo: remove this in 3.0.0 as it will be unnecessary. it's only being maintained now for backwards compat with 2.0 APIs.
        public static IAppBuilder ConfigureIdentityServerIssuer(this IAppBuilder app, IdentityServerOptions options)
        {
            if (app == null) throw new ArgumentNullException("app");
            if (options == null) throw new ArgumentNullException("options");

            if (options.IssuerUri.IsPresent())
            {
                options.DynamicallyCalculatedIssuerUri = options.IssuerUri;
            }
            else
            { 
                Action<IOwinContext> op = ctx =>
                {
                    var uri = ctx.Environment.GetIdentityServerBaseUrl();
                    if (uri.EndsWith("/")) uri = uri.Substring(0, uri.Length - 1);
                    options.DynamicallyCalculatedIssuerUri = uri;
                };

                app.Use(async (ctx, next) =>
                {
                    if (op != null)
                    {
                        var tmp = op;
                        op = null;
                        tmp(ctx);
                    }

                    await next();
                });
            }

            return app;
        }
    }
}
