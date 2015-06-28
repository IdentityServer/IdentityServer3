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
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace IdentityServer3.Core.Logging
{
    internal class RequestResponseLogger : DelegatingHandler
    {
        static readonly ILog Logger = LogProvider.GetLogger("HTTP Logging");

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var reqLog = new
            {
                Method = request.Method,
                Url = request.RequestUri.AbsoluteUri,
                Headers = request.Headers,
                Body = await request.GetOwinContext().Request.ReadBodyAsStringAsync()
            };

            Logger.DebugFormat("HTTP Request\n{0}", LogSerializer.Serialize(reqLog));

            var response = await base.SendAsync(request, cancellationToken);

            string body = "";

            if (response.Content != null)
            {
                body = await response.Content.ReadAsStringAsync();
            }

            var respLog = new
            {
                StatusCode = response.StatusCode,
                Headers = response.Headers,
                Body = body
            };

            Logger.DebugFormat("HTTP Response\n{0}", LogSerializer.Serialize(respLog));

            return response;
        }
    }
}
