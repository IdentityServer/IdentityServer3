﻿/*
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

using Thinktecture.IdentityServer.Core.Extensions;

namespace Owin
{
    internal static class ConfigureIdentityServerBaseUrlExtension
    {
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
                    publicOrigin = request.Uri.Scheme + "://" + request.Host.Value;
                }
                
                ctx.Environment.SetIdentityServerHost(publicOrigin);
                ctx.Environment.SetIdentityServerBasePath(ctx.Request.PathBase.Value.EnsureTrailingSlash());

                await next();
            });

            return app;
        }
    }
}
