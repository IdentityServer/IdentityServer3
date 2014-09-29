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
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using Thinktecture.IdentityServer.Core.Configuration;
using Thinktecture.IdentityServer.Core.Extensions;
using Thinktecture.IdentityServer.Core.Logging;

namespace Thinktecture.IdentityServer.Core.Authentication
{
    public class SignInMessageCookie
    {
        IOwinContext ctx;
        IdentityServerOptions options;
        public SignInMessageCookie(IDictionary<string, object> env, IdentityServerOptions options)
        {
            if (env == null) throw new ArgumentNullException("env");
            if (options == null) throw new ArgumentNullException("options");

            this.ctx = new OwinContext(env);
            this.options = options;
        }

        public SignInMessageCookie(IOwinContext ctx, IdentityServerOptions options)
        {
            if (ctx == null) throw new ArgumentNullException("ctx");
            if (options == null) throw new ArgumentNullException("options");
            
            this.ctx = ctx;
            this.options = options;
        }

        public const string SignInMessageCookieName = "{0}.idsrv.signin.{1}";

        private string GetCookieName(string id)
        {
            return String.Format(SignInMessageCookieName, 
                options.AuthenticationOptions.CookieOptions.Prefix,
                id);
        }
        
        private IEnumerable<string> GetCookieNames()
        {
            var key = GetCookieName(null);
            foreach (var cookie in ctx.Request.Cookies)
            {
                if (cookie.Key.StartsWith(key))
                {
                    yield return cookie.Key;
                }
            }
        }

        private string Protect(SignInMessage message)
        {
            if (message == null) throw new ArgumentNullException("message");

            return message.Protect(options.DataProtector);
        }

        private SignInMessage Unprotect(string data)
        {
            if (data == null) throw new ArgumentNullException("data");
            
            return SignInMessage.Unprotect(data, options.DataProtector);
        }

        private bool Secure
        {
            get
            {
                return ctx.Request.Uri.Scheme == Uri.UriSchemeHttps;
            }
        }

        public void Write(SignInMessage message)
        {
            var name = GetCookieName(message.Id);
            var data = Protect(message);

            ctx.Response.Cookies.Append(
                name,
                data,
                new Microsoft.Owin.CookieOptions
                {
                    HttpOnly = true,
                    Secure = Secure
                });
        }

        public SignInMessage Read(string id)
        {
            var name = GetCookieName(id);
            var data = ctx.Request.Cookies[name];
            var message = Unprotect(data);
            
            return message;
        }

        public void Clear(string id)
        {
            var name = GetCookieName(id);

            ctx.Response.Cookies.Append(
                name,
                ".",
                new Microsoft.Owin.CookieOptions
                {
                    Expires = DateTime.UtcNow.AddYears(-1),
                    HttpOnly = true,
                    Secure = Secure
                });
        }

        public void ClearAll()
        {
            var names = GetCookieNames();
            foreach (var name in names)
            {
                ctx.Response.Cookies.Append(
                    name,
                    ".",
                    new Microsoft.Owin.CookieOptions
                    {
                        Expires = DateTime.UtcNow.AddYears(-1),
                        HttpOnly = true,
                        Secure = Secure
                    });
            }
        }
    }
}