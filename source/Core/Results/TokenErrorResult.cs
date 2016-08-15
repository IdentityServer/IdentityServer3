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
using System.Linq;
using IdentityServer3.Core.Logging;
using Newtonsoft.Json;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using Newtonsoft.Json.Linq;

namespace IdentityServer3.Core.Results
{
    internal class TokenErrorResult : IHttpActionResult
    {
        private readonly static ILog Logger = LogProvider.GetCurrentClassLogger();

        private readonly static JsonSerializer Serializer = new JsonSerializer
        {
            DefaultValueHandling = DefaultValueHandling.Ignore,
            NullValueHandling = NullValueHandling.Ignore
        };

        public IDictionary<string, object> CustomResponseParamaters { get; set; }
        public string Error { get; internal set; }
        public string ErrorDescription { get; internal set; }

        public TokenErrorResult(string error)
        {
            Error = error;
            CustomResponseParamaters = new Dictionary<string, object>();
        }

        public TokenErrorResult(string error, string errorDescription)
        {
            Error = error;
            ErrorDescription = errorDescription;
            CustomResponseParamaters = new Dictionary<string, object>();
        }

        public TokenErrorResult(string error, IDictionary<string, object> customResponseParamaters)
        {
            Error = error;
            CustomResponseParamaters = customResponseParamaters;
        }

        public TokenErrorResult(string error, string errorDescription, IDictionary<string, object> customResponseParamaters)
        {
            Error = error;
            ErrorDescription = errorDescription;
            CustomResponseParamaters = customResponseParamaters;
        }

        public Task<HttpResponseMessage> ExecuteAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult(Execute());
        }

        private HttpResponseMessage Execute()
        {
            var dto = new ErrorDto
            {
                error = Error,
                error_description = ErrorDescription
            };

            var jobject = JObject.FromObject(dto, Serializer);

            // custom entries
            if (CustomResponseParamaters != null && CustomResponseParamaters.Any())
            {
                foreach (var item in CustomResponseParamaters)
                {
                    JToken token;
                    if (jobject.TryGetValue(item.Key, out token))
                    {
                        throw new Exception("Item does already exist - cannot add it via a custom entry: " + item.Key);
                    }

                    jobject.Add(new JProperty(item.Key, item.Value));
                }
            }

            var response = new HttpResponseMessage(HttpStatusCode.BadRequest)
            {
                Content = new StringContent(jobject.ToString(Formatting.None), Encoding.UTF8, "application/json")
            };

            Logger.Info("Returning error: " + Error);
            return response;
        }

        internal class ErrorDto
        {
            public string error { get; set; }
            [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
            public string error_description { get; set; }
        }
    }
}