﻿/*
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
using System.Linq;
using System.Security.Cryptography;
using Thinktecture.IdentityModel;
using Thinktecture.IdentityServer.Core.Extensions;
using Thinktecture.IdentityServer.Core.Logging;
using Thinktecture.IdentityServer.Core.Models;

namespace Thinktecture.IdentityServer.Core.Configuration.Hosting
{
    public class MessageCookie<TMessage>
        where TMessage : Message
    {
        private static readonly ILog Logger = LogProvider.GetCurrentClassLogger();

        readonly IOwinContext ctx;
        readonly IdentityServerOptions options;

        public MessageCookie(IDictionary<string, object> env, IdentityServerOptions options)
        {
            if (env == null) throw new ArgumentNullException("env");
            if (options == null) throw new ArgumentNullException("options");

            this.ctx = new OwinContext(env);
            this.options = options;
        }

        public MessageCookie(IOwinContext ctx, IdentityServerOptions options)
        {
            if (ctx == null) throw new ArgumentNullException("ctx");
            if (options == null) throw new ArgumentNullException("options");
            
            this.ctx = ctx;
            this.options = options;
        }

        public string MessageType
        {
            get { return typeof(TMessage).Name; }
        }

        public string Protect(IDataProtector protector, TMessage message)
        {
            var settings = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                DefaultValueHandling = DefaultValueHandling.Ignore,
            };

            var json = JsonConvert.SerializeObject(message, settings);
            Logger.DebugFormat("Protecting message: {0}", json);

            return protector.Protect(json, MessageType);
        }

        public TMessage Unprotect(string data, IDataProtector protector)
        {
            var json = protector.Unprotect(data, MessageType);
            var message = JsonConvert.DeserializeObject<TMessage>(json);
            return message;
        }

        public string GetCookieName(string id = null)
        {
            return String.Format("{0}{1}.{2}", 
                options.AuthenticationOptions.CookieOptions.Prefix, 
                MessageType, 
                id);
        }
        
        public string CookiePath
        {
            get
            {
                var path = ctx.Request.Environment.GetIdentityServerBasePath();
                if (path.EndsWith("/")) path = path.Substring(0, path.Length - 1);
                if (String.IsNullOrWhiteSpace(path)) path = "/";
                return path;
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

        public void ClearAll()
        {
            var names = GetCookieNames();
            foreach (var name in names)
            {
                ClearByCookieName(name);
            }
        }
        
        void ClearByCookieName(string name)
        {
            ctx.Response.Cookies.Append(
                name,
                ".",
                new Microsoft.Owin.CookieOptions
                {
                    Expires = DateTime.UtcNow.AddYears(-1),
                    HttpOnly = true,
                    Secure = Secure,
                    Path = CookiePath
                });
        }

        private long GetCookieRank(string name)
        {   // empty and invalid cookies are considered to be the oldest:
            var rank = DateTime.MinValue.Ticks;
            try
            {
                var m = ReadByCookieName(name);
                if (m != null)
                {   // valid cookies are ranked based on their creation time:
                    rank = m.Requested;
                }
            }
            catch (CryptographicException e)
            {   // cookie was protected with a different key/algorithm
                Logger.DebugFormat("Unable to decrypt cookie {0}: {1}", name, e.Message);
            }
            return rank;
        }

        private void ClearOverflow()
        {
            var names = GetCookieNames();
            if (names.Count() > Constants.SignInMessageThreshold)
            {
                var rankedCookieNames =
                    from name in names
                    let rank = GetCookieRank(name)
                    orderby rank descending
                    select name;

                var purge = rankedCookieNames.Skip(Constants.SignInMessageThreshold);
                foreach (var name in purge)
                {
                    ClearByCookieName(name);
                }
            }
        }
    }
}