/*
 * Copyright 2014 Dominick Baier, Brock Allen
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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Thinktecture.IdentityModel;
using Thinktecture.IdentityServer.Core.Extensions;

namespace Thinktecture.IdentityServer.Core.Configuration.Hosting
{
    public class SessionCookie
    {
        IOwinContext context;
        IdentityServerOptions identityServerOptions;

        public SessionCookie(IOwinContext ctx, IdentityServerOptions options)
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
            var path = context.Request.Environment.GetIdentityServerBasePath();
            if (path.EndsWith("/")) path = path.Substring(0, path.Length - 1);
            if (String.IsNullOrWhiteSpace(path)) path = "/";

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
            options.Expires = DateTime.UtcNow.AddYears(-1);
            
            var name = GetCookieName();
            context.Response.Cookies.Append(name, ".", options);
        }
    }
}
