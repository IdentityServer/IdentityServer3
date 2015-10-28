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

namespace Owin
{
    /// <summary>
    /// Configure extensions for HSTS support
    /// </summary>
    public static class UseHstsExtension
    {
        /// <summary>
        /// Enables HTTP Strict Transport Security (HSTS) for the hosting application.
        /// </summary>
        /// <param name="app">The application.</param>
        /// <param name="duration">The duration the HSTS header should be cached in the client browser. <c>TimeSpan.Zero</c> will clear the cached value.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException">app</exception>
        /// <exception cref="System.ArgumentException">duration cannot be below zero</exception>
        public static IAppBuilder UseHsts(this IAppBuilder app, TimeSpan duration)
        {
            if (app == null) throw new ArgumentNullException("app");
            if (duration < TimeSpan.Zero) throw new ArgumentException("duration cannot be below zero");

            if (duration >= TimeSpan.Zero)
            {
                var seconds = (int)duration.TotalSeconds;
                app.Use(async (ctx, next) =>
                {
                    if (ctx.Request.IsSecure)
                    {
                        ctx.Response.Headers.Append("Strict-Transport-Security", "max-age=" + seconds);
                    }
                    await next();
                });
            }

            return app;
        }

        /// <summary>
        /// Enables HTTP Strict Transport Security (HSTS) for the hosting application.
        /// </summary>
        /// <param name="app">The application.</param>
        /// <param name="days">The number of days the HSTS header should be cached in the client browser. A value of zero will clear the cached value.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentException">days cannot be below zero</exception>
        /// <exception cref="System.ArgumentNullException">app</exception>
        public static IAppBuilder UseHsts(this IAppBuilder app, int days = 30)
        {
            if (days < 0) throw new ArgumentException("days cannot be below zero");

            return app.UseHsts(TimeSpan.FromDays(days));
        }
    }
}