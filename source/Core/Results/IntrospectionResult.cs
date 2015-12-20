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

using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;

namespace IdentityServer3.Core.Results
{
    internal class IntrospectionResult : IHttpActionResult
    {
        private readonly static JsonMediaTypeFormatter Formatter = new JsonMediaTypeFormatter();

        public Dictionary<string, object> Result { get; private set; }

        public IntrospectionResult(Dictionary<string, object> result)
        {
            Result = result;
        }

        public Task<HttpResponseMessage> ExecuteAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult(Execute());
        }

        private HttpResponseMessage Execute()
        {
            var content = new ObjectContent<Dictionary<string, object>>(Result, Formatter);
            var message = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = content
            };

            return message;
        }
    }
}