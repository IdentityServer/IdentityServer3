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

using IdentityModel;
using IdentityServer3.Core.Extensions;
using Microsoft.Owin;
using System;
using System.ComponentModel;

#pragma warning disable 1591

namespace IdentityServer3.Core.Configuration.Hosting
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

        public virtual void IssueSessionId(bool? persistent, DateTimeOffset? expires = null)
        {
            context.Response.Cookies.Append(
                GetCookieName(), CryptoRandom.CreateUniqueId(), 
                CreateCookieOptions(persistent, expires));
        }

        private Microsoft.Owin.CookieOptions CreateCookieOptions(bool? persistent, DateTimeOffset? expires = null)
        {
            var path = context.Request.Environment.GetIdentityServerBasePath().CleanUrlPath();

            var options = new Microsoft.Owin.CookieOptions
            {
                HttpOnly = false,
                Secure = context.Request.IsSecure,
                Path = path
            };

            if (persistent != false)
            {
                if (persistent == true || this.identityServerOptions.AuthenticationOptions.CookieOptions.IsPersistent)
                {
                    if (persistent == true)
                    {
                        expires = expires ?? DateTimeHelper.UtcNow.Add(this.identityServerOptions.AuthenticationOptions.CookieOptions.RememberMeDuration);
                    }
                    else
                    {
                        expires = expires ?? DateTimeHelper.UtcNow.Add(this.identityServerOptions.AuthenticationOptions.CookieOptions.ExpireTimeSpan);
                    }
                    options.Expires = expires.Value.UtcDateTime;
                }
            }

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
            var options = CreateCookieOptions(false);
            options.Expires = DateTimeHelper.UtcNow.AddYears(-1);
            
            var name = GetCookieName();
            context.Response.Cookies.Append(name, ".", options);
        }
    }
}
