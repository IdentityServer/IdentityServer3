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

using IdentityServer3.Core.Extensions;
using IdentityServer3.Core.Logging;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;

namespace IdentityServer3.Core.Results
{
    internal class ProtectedResourceErrorResult : IHttpActionResult
    {
        private readonly static ILog Logger = LogProvider.GetCurrentClassLogger();
        private readonly string _error;
        private readonly string _errorDescription;

        public ProtectedResourceErrorResult(string error, string errorDescription = null)
        {
            _error = error;
            _errorDescription = errorDescription;
        }

        public Task<HttpResponseMessage> ExecuteAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult(Execute());
        }

        private HttpResponseMessage Execute()
        {
            var response = new HttpResponseMessage(HttpStatusCode.Unauthorized);
            if (Constants.ProtectedResourceErrorStatusCodes.ContainsKey(_error))
            {
                response.StatusCode = Constants.ProtectedResourceErrorStatusCodes[_error];
            }

            var parameter = string.Format("error=\"{0}\"", _error);
            if (_errorDescription.IsPresent())
            {
                parameter = string.Format("{0}, error_description=\"{1}\"",
                    parameter, _errorDescription);
            }

            var header = new AuthenticationHeaderValue("Bearer", parameter);
            response.Headers.WwwAuthenticate.Add(header);

            Logger.Info("Returning error: " + _error);
            return response;
        }
    }
}