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

using IdentityServer3.Core.Configuration.Hosting;
using IdentityServer3.Core.Extensions;
using IdentityServer3.Core.Models;
using System;

namespace Owin
{
    internal static class SignOutMessageCookieExtension
    {
        public static IAppBuilder ConfigureSignOutMessageCookie(this IAppBuilder app)
        {
            if (app == null) throw new ArgumentNullException("app");

            return app.Use(async (context, next) =>
            {
                // need to do the ResolveDependency here before OnSendingHeaders
                // since OnSendingHeaders might run after DI and autofac have cleaned up
                var signOutMessageCookie = context.ResolveDependency<MessageCookie<SignOutMessage>>();

                context.Response.OnSendingHeaders(state =>
                {
                    // this is needed to remove sign out message cookie if we're not redirecting to upstream IdP
                    // we don't know until we're on the way out of the pipeline after the external IdP middleware
                    // has run. if we see our flag and 200 response, then we remove the cookie normally.
                    // if we don't see 200, then we leave it and expect a post logout callback from the upstream
                    // IdP at which point we can show our logged out page and finish processing the signout message
                    // which might contain client info, post logout redirects, etc.
                    context.ProcessRemovalOfSignOutMessageCookie(signOutMessageCookie);
                }, null);

                await next();
            });
        }
    }
}