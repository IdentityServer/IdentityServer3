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

using System;
using System.Linq;
using Thinktecture.IdentityServer.Core.Extensions;

namespace Owin
{
    internal static class ConfigureIdentityServerBaseUrlExtension
    {
        private const string X_FORWARDED_HOST_HEADER_KEY = "X-Forwarded-Host";
        private const string X_FORWARDED_PROTO_HEADER_KEY = "X-Forwarded-Proto";
        public static IAppBuilder ConfigureIdentityServerBaseUrl(this IAppBuilder app, string publicOrigin)
        {
            if (publicOrigin.IsPresent())
            {
                publicOrigin = publicOrigin.RemoveTrailingSlash();
            }

            app.Use(async (ctx, next) =>
            {
                var request = ctx.Request;
                if (publicOrigin.IsMissing())
                {
                  var xForwardedHosts = ctx.Request.Headers.FirstOrDefault(h => h.Key.ToUpper() == X_FORWARDED_HOST_HEADER_KEY.ToUpper()).Value;
                  var xForwardedHost = (xForwardedHosts == null) ? null : xForwardedHosts.SingleOrDefault();
                  var host = xForwardedHost ?? request.Host.Value;
                
                  var xForwardedProtos = ctx.Request.Headers.FirstOrDefault(h => h.Key.ToUpper() == X_FORWARDED_PROTO_HEADER_KEY.ToUpper()).Value;
                  var xForwardedProto = (xForwardedProtos == null) ? null : xForwardedProtos.SingleOrDefault();
                  var proto = xForwardedProto ?? request.Uri.Scheme;
                  
                  publicOrigin = proto + "://" + host;
                }
                
                ctx.Environment.SetIdentityServerHost(publicOrigin);
                ctx.Environment.SetIdentityServerBasePath(ctx.Request.PathBase.Value.EnsureTrailingSlash());

                await next();
            });

            return app;
        }
    }
}
