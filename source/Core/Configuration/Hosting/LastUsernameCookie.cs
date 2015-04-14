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
using IdentityServer3.Core.Logging;
using Microsoft.Owin;
using System;
using System.ComponentModel;
using System.Security.Cryptography;
using System.Text;


#pragma warning disable 1591

namespace IdentityServer3.Core.Configuration.Hosting
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class LastUserNameCookie
    {
        private static readonly ILog Logger = LogProvider.GetCurrentClassLogger();

        const string LastUsernameCookieName = "idsvr.username";

        readonly IOwinContext ctx;
        readonly IdentityServerOptions options;

        internal LastUserNameCookie(IOwinContext ctx, IdentityServerOptions options)
        {
            if (ctx == null) throw new ArgumentNullException("ctx");
            if (options == null) throw new ArgumentNullException("options");
            
            this.ctx = ctx;
            this.options = options;
        }

        internal string GetValue()
        {
            if (options.AuthenticationOptions.RememberLastUsername)
            {
                try
                {
                    var cookieName = options.AuthenticationOptions.CookieOptions.Prefix + LastUsernameCookieName;
                    var value = ctx.Request.Cookies[cookieName];

                    var bytes = Base64Url.Decode(value);
                    try
                    {
                        bytes = options.DataProtector.Unprotect(bytes, cookieName);
                    }
                    catch(CryptographicException)
                    {
                        SetValue(null);
                        return null;
                    }
                    value = Encoding.UTF8.GetString(bytes);

                    return value;
                }
                catch
                {
                    SetValue(null);
                }
            }
            return null;
        }

        internal void SetValue(string username)
        {
            if (options.AuthenticationOptions.RememberLastUsername)
            {
                var cookieName = options.AuthenticationOptions.CookieOptions.Prefix + LastUsernameCookieName;
                var secure = ctx.Request.Scheme == Uri.UriSchemeHttps;
                var path = ctx.Request.Environment.GetIdentityServerBasePath().CleanUrlPath();

                var cookieOptions = new Microsoft.Owin.CookieOptions
                {
                    HttpOnly = true,
                    Secure = secure,
                    Path = path
                };

                if (!String.IsNullOrWhiteSpace(username))
                {
                    var bytes = Encoding.UTF8.GetBytes(username);
                    bytes = options.DataProtector.Protect(bytes, cookieName);
                    username = Base64Url.Encode(bytes);
                    cookieOptions.Expires = DateTimeHelper.UtcNow.AddYears(1);
                }
                else
                {
                    username = ".";
                    cookieOptions.Expires = DateTimeHelper.UtcNow.AddYears(-1);
                }

                ctx.Response.Cookies.Append(cookieName, username, cookieOptions);
            }
        }
    }
}