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

using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using Thinktecture.IdentityServer.Core.Configuration;
using Thinktecture.IdentityServer.Core.Extensions;
using Thinktecture.IdentityServer.Core.Logging;

namespace Thinktecture.IdentityServer.Core.Authentication
{
    public class LoginResult : IHttpActionResult
    {
        private readonly static ILog Logger = LogProvider.GetCurrentClassLogger();

        private readonly SignInMessage _message;
        private readonly IDictionary<string, object> _env;
        private readonly IDataProtector _protector;
        private readonly int _messageExpiration;

        public LoginResult(SignInMessage message, IDictionary<string, object> env, IDataProtector protector, int messageExpiration)
        {
            _message = message;
            _env = env;
            _protector = protector;

            if (messageExpiration <= 0) messageExpiration = Constants.DefaultSignInMessageExpiration;
            _messageExpiration = messageExpiration;
        }

        public Task<HttpResponseMessage> ExecuteAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult(Execute());
        }

        private HttpResponseMessage Execute()
        {
            var response = new HttpResponseMessage(HttpStatusCode.Redirect);

            try
            {
                var sim = _message.Protect(_messageExpiration, _protector);
                var url = _env.GetIdentityServerBaseUrl() + Constants.RoutePaths.Login;
                url += "?message=" + sim;

                var uri = new Uri(url);
                response.Headers.Location = uri;
            }
            catch
            {
                response.Dispose();
                throw;
            }

            Logger.Info("Redirecting to login page");
            return response;
        }
    }
}