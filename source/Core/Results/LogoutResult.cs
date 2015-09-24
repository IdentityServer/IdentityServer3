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

using IdentityServer3.Core.Configuration;
using IdentityServer3.Core.Configuration.Hosting;
using IdentityServer3.Core.Extensions;
using IdentityServer3.Core.Logging;
using IdentityServer3.Core.Models;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;

namespace IdentityServer3.Core.Results
{
    internal class LogoutResult : IHttpActionResult
    {
        private readonly static ILog Logger = LogProvider.GetCurrentClassLogger();

        private readonly SignOutMessage message;
        private readonly IDictionary<string, object> env;
        private readonly IdentityServerOptions options;

        public static string GetRedirectUrl(SignOutMessage message, IDictionary<string, object> env, IdentityServerOptions options)
        {
            var result = new LogoutResult(message, env, options);
            var response = result.Execute();

            return response.Headers.Location.AbsoluteUri;
        }

        public LogoutResult(SignOutMessage message, IDictionary<string, object> env, IdentityServerOptions options)
        {
            if (env == null) throw new ArgumentNullException("env");
            if (options == null) throw new ArgumentNullException("options");

            this.env = env;
            this.options = options;

            this.message = message;
        }

        public Task<HttpResponseMessage> ExecuteAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult(Execute());
        }

        private HttpResponseMessage Execute()
        {
            Logger.Info("Redirecting to logout page");

            var url = env.GetIdentityServerBaseUrl() + Constants.RoutePaths.Logout;

            if (message != null)
            {
                var cookie = new MessageCookie<SignOutMessage>(this.env, this.options);
                var id = cookie.Write(this.message);

                url = url.AddQueryString("id=" + id);
            }

            var uri = new Uri(url);

            var response = new HttpResponseMessage(HttpStatusCode.Redirect);
            response.Headers.Location = uri;
            return response;
        }
    }
}