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
using IdentityServer3.Core.Models;
using Microsoft.Owin;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Security.Cryptography;

#pragma warning disable 1591

namespace IdentityServer3.Core.Configuration.Hosting
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class MessageCookie<TMessage>
        where TMessage : Message
    {
        private static readonly ILog Logger = LogProvider.GetCurrentClassLogger();

        static readonly JsonSerializerSettings settings = new JsonSerializerSettings
        {
            NullValueHandling = NullValueHandling.Ignore,
            DefaultValueHandling = DefaultValueHandling.Ignore,
        };

        readonly IOwinContext ctx;
        readonly IdentityServerOptions options;

        internal MessageCookie(IDictionary<string, object> env, IdentityServerOptions options)
        {
            if (env == null) throw new ArgumentNullException("env");
            if (options == null) throw new ArgumentNullException("options");

            this.ctx = new OwinContext(env);
            this.options = options;
        }

        internal MessageCookie(IOwinContext ctx, IdentityServerOptions options)
        {
            if (ctx == null) throw new ArgumentNullException("ctx");
            if (options == null) throw new ArgumentNullException("options");
            
            this.ctx = ctx;
            this.options = options;
        }

        string MessageType
        {
            get { return typeof(TMessage).Name; }
        }

        string Protect(IDataProtector protector, TMessage message)
        {
            var json = JsonConvert.SerializeObject(message, settings);
            Logger.DebugFormat("Protecting message: {0}", json);

            return protector.Protect(json, MessageType);
        }

        TMessage Unprotect(string data, IDataProtector protector)
        {
            var json = protector.Unprotect(data, MessageType);
            var message = JsonConvert.DeserializeObject<TMessage>(json);
            return message;
        }

        string GetCookieName(string id = null)
        {
            return String.Format("{0}{1}.{2}", 
                options.AuthenticationOptions.CookieOptions.Prefix, 
                MessageType, 
                id);
        }
        
        string CookiePath
        {
            get
            {
                return ctx.Request.Environment.GetIdentityServerBasePath().CleanUrlPath();
            }
        }

        private IEnumerable<string> GetCookieNames()
        {
            var key = GetCookieName();
            foreach (var cookie in ctx.Request.Cookies)
            {
                if (cookie.Key.StartsWith(key))
                {
                    yield return cookie.Key;
                }
            }
        }

        private string Protect(TMessage message)
        {
            if (message == null) throw new ArgumentNullException("message");

            return Protect(options.DataProtector, message);
        }

        private TMessage Unprotect(string data)
        {
            if (data == null) throw new ArgumentNullException("data");
            
            return Unprotect(data, options.DataProtector);
        }

        private bool Secure
        {
            get
            {
                return ctx.Request.Scheme == Uri.UriSchemeHttps;
            }
        }

        public string Write(TMessage message)
        {
            ClearOverflow();

            if (message == null) throw new ArgumentNullException("message");

            var id = CryptoRandom.CreateUniqueId();
            var name = GetCookieName(id);
            var data = Protect(message);

            ctx.Response.Cookies.Append(
                name,
                data,
                new Microsoft.Owin.CookieOptions
                {
                    HttpOnly = true,
                    Secure = Secure,
                    Path = CookiePath
                });
            return id;
        }

        public TMessage Read(string id)
        {
            if (String.IsNullOrWhiteSpace(id)) return null;

            var name = GetCookieName(id);
            return ReadByCookieName(name);
        }

        TMessage ReadByCookieName(string name)
        {
            var data = ctx.Request.Cookies[name];
            if (!String.IsNullOrWhiteSpace(data))
            {
                return Unprotect(data);
            }
            return null;
        }

        public void Clear(string id)
        {
            var name = GetCookieName(id);
            ClearByCookieName(name);
        }

        void ClearByCookieName(string name)
        {
            ctx.Response.Cookies.Append(
                name,
                ".",
                new Microsoft.Owin.CookieOptions
                {
                    Expires = DateTimeHelper.UtcNow.AddYears(-1),
                    HttpOnly = true,
                    Secure = Secure,
                    Path = CookiePath
                });
        }

        private long GetCookieRank(string name)
        {   
            // empty and invalid cookies are considered to be the oldest:
            var rank = DateTimeOffset.MinValue.Ticks;

            try
            {
                var message = ReadByCookieName(name);
                if (message != null)
                {   // valid cookies are ranked based on their creation time:
                    rank = message.Created;
                }
            }
            catch (CryptographicException e)
            {   
                // cookie was protected with a different key/algorithm
                Logger.DebugFormat("Unable to decrypt cookie {0}: {1}", name, e.Message);
            }
            
            return rank;
        }

        private void ClearOverflow()
        {
            var names = GetCookieNames();
            var toKeep = options.AuthenticationOptions.SignInMessageThreshold;

            if (names.Count() >= toKeep)
            {
                var rankedCookieNames =
                    from name in names
                    let rank = GetCookieRank(name)
                    orderby rank descending
                    select name;

                var purge = rankedCookieNames.Skip(Math.Max(0, toKeep - 1));
                foreach (var name in purge)
                {
                    ClearByCookieName(name);
                }
            }
        }
    }
}