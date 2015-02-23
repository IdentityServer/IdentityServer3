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

using Microsoft.Owin;
using System.ComponentModel;
using Thinktecture.IdentityModel;
using Thinktecture.IdentityServer.Core.Extensions;

#pragma warning disable 1591

namespace Thinktecture.IdentityServer.Core.Configuration.Hosting
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class SessionCookie
    {
        readonly IOwinContext context;
        readonly IdentityServerOptions identityServerOptions;

        protected internal SessionCookie(IOwinContext ctx, IdentityServerOptions options)
        {
            this.context = ctx;
            this.identityServerOptions = options;
        }

        public virtual void IssueSessionId()
        {
            context.Response.Cookies.Append(
                GetCookieName(), CryptoRandom.CreateUniqueId(), 
                CreateCookieOptions());
        }

        private Microsoft.Owin.CookieOptions CreateCookieOptions()
        {
            var path = context.Request.Environment.GetIdentityServerBasePath().CleanUrlPath();

            var options = new Microsoft.Owin.CookieOptions
            {
                HttpOnly = false,
                Secure = context.Request.IsSecure,
                Path = path
            };
            return options;
        }

        private string GetCookieName()
        {
            return identityServerOptions.AuthenticationOptions.CookieOptions.GetSessionCookieName();
        }

        public virtual string GetSessionId()
        {
            return context.Request.Cookies[GetCookieName()];
        }

        public virtual void ClearSessionId()
        {
            var options = CreateCookieOptions();
            options.Expires = DateTimeHelper.UtcNow.AddYears(-1);
            
            var name = GetCookieName();
            context.Response.Cookies.Append(name, ".", options);
        }
    }
}
