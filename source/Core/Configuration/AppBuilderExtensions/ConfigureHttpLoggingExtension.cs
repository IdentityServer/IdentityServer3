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
using IdentityServer3.Core.Logging;
using Microsoft.Owin;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Owin
{
    static class ConfigureHttpLoggingExtension
    {
        static readonly ILog Logger = LogProvider.GetLogger("HTTP Logging");

        public static IAppBuilder ConfigureHttpLogging(this IAppBuilder app, LoggingOptions options)
        {
            if (options.EnableHttpLogging)
            {
                app.Use(async (ctx, next) =>
                {
                    await LogRequest(ctx.Request);

                    var oldStream = ctx.Response.Body;
                    var ms = ctx.Response.Body = new MemoryStream();

                    try
                    {
                        await next();
                        await LogResponse(ctx.Response);

                        ctx.Response.Body = oldStream;
                        await ms.CopyToAsync(oldStream);
                    }
                    catch(Exception ex)
                    {
                        Logger.DebugException("HTTP Response Exception", ex);
                        throw;
                    }
                });
            }

            return app;
        }

        private static async Task LogRequest(IOwinRequest request)
        {
            var reqLog = new
            {
                Method = request.Method,
                Url = request.Uri.AbsoluteUri,
                Headers = request.Headers,
                Body = await request.ReadBodyAsStringAsync()
            };

            Logger.Debug("HTTP Request" + Environment.NewLine + LogSerializer.Serialize(reqLog));
        }

        private static async Task LogResponse(IOwinResponse response)
        {
            var respLog = new
            {
                StatusCode = response.StatusCode,
                Headers = response.Headers,
                Body = await response.ReadBodyAsStringAsync()
            };

            Logger.Debug("HTTP Response" + Environment.NewLine + LogSerializer.Serialize(respLog));
        }
    }
}
