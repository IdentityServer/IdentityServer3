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
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using Thinktecture.IdentityServer.Core.App_Packages.LibLog._2._0;
using Thinktecture.IdentityServer.Core.Configuration;
using Thinktecture.IdentityServer.Core.Configuration.Hosting;
using Thinktecture.IdentityServer.Core.Extensions;
using Thinktecture.IdentityServer.Core.Models;

namespace Thinktecture.IdentityServer.Core.Results
{
    internal class LogoutResult : IHttpActionResult
    {
        private readonly static ILog Logger = LogProvider.GetCurrentClassLogger();

        private readonly SignOutMessage _message;
        private readonly IDictionary<string, object> _env;
        private readonly IdentityServerOptions _options;

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

            _env = env;
            _options = options;

            _message = message;
        }

        public Task<HttpResponseMessage> ExecuteAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult(Execute());
        }

        private HttpResponseMessage Execute()
        {
            Logger.Info("Redirecting to logout page");

            var url = _env.GetIdentityServerBaseUrl() + Constants.RoutePaths.LOGOUT;

            if (_message != null)
            {
                var cookie = new MessageCookie<SignOutMessage>(_env, _options);
                var id = cookie.Write(_message);

                url = url.AddQueryString("id=" + id);
            }

            var uri = new Uri(url);

            var response = new HttpResponseMessage(HttpStatusCode.Redirect);
            response.Headers.Location = uri;
            return response;
        }
    }
}