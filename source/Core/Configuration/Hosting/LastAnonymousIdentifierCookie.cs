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
using System.ComponentModel;
using System.Security.Cryptography;
using System.Text;

using IdentityModel;
using IdentityServer3.Core.Extensions;

using Microsoft.Owin;

namespace IdentityServer3.Core.Configuration.Hosting
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class LastAnonymousIdentifierCookie
    {
        private const string LastAnonymousIdentifierName = "idsvr.anonymousid";

        private readonly IOwinContext ctx;

        private readonly IdentityServerOptions options;

        internal LastAnonymousIdentifierCookie(IOwinContext ctx, IdentityServerOptions options)
        {
            if (ctx == null)
            {
                throw new ArgumentNullException("ctx");
            }
            if (options == null)
            {
                throw new ArgumentNullException("options");
            }

            this.ctx = ctx;
            this.options = options;
        }

        internal string GetValue()
        {
            if (!this.options.AuthenticationOptions.RememberLastIssuedAnonymousIdentifier)
            {
                return null;
            }

            try
            {
                var cookieName = this.options.AuthenticationOptions.CookieOptions.Prefix + LastAnonymousIdentifierName;
                var value = this.ctx.Request.Cookies[cookieName];

                var bytes = Base64Url.Decode(value);
                try
                {
                    bytes = this.options.DataProtector.Unprotect(bytes, cookieName);
                }
                catch (CryptographicException)
                {
                    this.SetValue(null);
                    return null;
                }

                value = Encoding.UTF8.GetString(bytes);

                return value;
            }
            catch
            {
                this.SetValue(null);
            }
            return null;
        }

        internal void SetValue(string anonymousId)
        {
            if (!this.options.AuthenticationOptions.RememberLastIssuedAnonymousIdentifier)
            {
                return;
            }

            var cookieName = this.options.AuthenticationOptions.CookieOptions.Prefix + LastAnonymousIdentifierName;
            var secure = this.ctx.Request.Scheme == Uri.UriSchemeHttps;
            var path = this.ctx.Request.Environment.GetIdentityServerBasePath().CleanUrlPath();

            var cookieOptions = new Microsoft.Owin.CookieOptions { HttpOnly = true, Secure = secure, Path = path };

            if (!String.IsNullOrWhiteSpace(anonymousId))
            {
                var bytes = Encoding.UTF8.GetBytes(anonymousId);
                bytes = this.options.DataProtector.Protect(bytes, cookieName);
                anonymousId = Base64Url.Encode(bytes);
            }
            else
            {
                anonymousId = ".";
            }

            this.ctx.Response.Cookies.Append(cookieName, anonymousId, cookieOptions);
        }

        internal string CreateNew()
        {
            var anonymousId = Guid.NewGuid().ToString();
            this.SetValue(anonymousId);
            return anonymousId;
        }
    }
}