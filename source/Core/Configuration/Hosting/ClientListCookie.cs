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
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IdentityServer3.Core.Configuration.Hosting
{
    internal class ClientListCookie
    {
        const string ClientListCookieName = "idsvr.clients";

        static readonly JsonSerializerSettings settings = new JsonSerializerSettings
        {
            NullValueHandling = NullValueHandling.Ignore,
            DefaultValueHandling = DefaultValueHandling.Ignore,
        };

        readonly IOwinContext ctx;
        readonly IdentityServerOptions options;

        public ClientListCookie(IOwinContext ctx, IdentityServerOptions options)
        {
            this.ctx = ctx;
            this.options = options;
        }

        public void Clear()
        {
            SetClients(null);
        }

        public void AddClient(string clientId)
        {
            if (options.Endpoints.EnableEndSessionEndpoint)
            {
                var clients = GetClients();
                if (!clients.Contains(clientId))
                {
                    var update = clients.ToList();
                    update.Add(clientId);
                    SetClients(update);
                }
            }
        }

        public IEnumerable<string> GetClients()
        {
            var value = GetCookie();
            if (String.IsNullOrWhiteSpace(value))
            {
                return Enumerable.Empty<string>();
            }

            return JsonConvert.DeserializeObject<string[]>(value, settings);
        }

        void SetClients(IEnumerable<string> clients)
        {
            string value = null;
            if (clients != null && clients.Any())
            {
                value = JsonConvert.SerializeObject(clients);
            }

            SetCookie(value);
        }

        string CookieName
        {
            get
            {
                return options.AuthenticationOptions.CookieOptions.GetCookieName(ClientListCookieName);
            }
        }

        string CookiePath
        {
            get
            {
                return ctx.Request.Environment.GetIdentityServerBasePath().CleanUrlPath();
            }
        }

        private bool Secure
        {
            get
            {
                return
                    options.AuthenticationOptions.CookieOptions.SecureMode == CookieSecureMode.Always ||
                    ctx.Request.Scheme == Uri.UriSchemeHttps;
            }
        }

        void SetCookie(string value)
        {
            DateTime? expires = null;
            if (String.IsNullOrWhiteSpace(value))
            {
                var existingValue = GetCookie();
                if (existingValue == null)
                {
                    // no need to write cookie to clear if we don't already have one
                    return;
                }

                value = ".";
                expires = DateTime.Now.AddYears(-1);
            }
            else
            {
                // encode the value
                var bytes = Encoding.UTF8.GetBytes(value);
                value = Base64Url.Encode(bytes);
            }

            var opts = new Microsoft.Owin.CookieOptions
            {
                HttpOnly = true,
                Secure = Secure,
                Path = CookiePath,
                Expires = expires
            };

            this.ctx.Response.Cookies.Append(CookieName, value, opts);
        }

        string GetCookie()
        {
            var value = this.ctx.Request.Cookies[CookieName];

            // the check here is to allow for handling cookies prior to manually encoding
            if (!String.IsNullOrWhiteSpace(value) && !value.StartsWith("["))
            {
                try
                {
                    var bytes = Base64Url.Decode(value);
                    value = Encoding.UTF8.GetString(bytes);
                }
                catch
                {
                }

                if (!value.StartsWith("["))
                {
                    // deal with double encoding or just invalid values
                    value = "";
                }
            }

            return value;
        }
    }
}
