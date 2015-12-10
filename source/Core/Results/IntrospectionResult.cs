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
using IdentityServer3.Core.Models;
using IdentityServer3.Core.Validation;
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
        private readonly static ILog Logger = LogProvider.GetCurrentClassLogger();
        private readonly static JsonMediaTypeFormatter Formatter = new JsonMediaTypeFormatter();

        public IntrospectionRequestValidationResult IntrospectionValidationResult { get; set; }
        public Scope Scope { get; set; }

        private readonly bool _sendInactive;

        public IntrospectionResult()
        {
            _sendInactive = true;
        }

        public IntrospectionResult(IntrospectionRequestValidationResult introspectionValidationResult, Scope scope)
        {
            IntrospectionValidationResult = introspectionValidationResult;
            Scope = scope;
        }

        public Task<HttpResponseMessage> ExecuteAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult(Execute());
        }

        private HttpResponseMessage Execute()
        {
            if (_sendInactive == true)
            {
                var inactiveContent = new ObjectContent<object>(new { active = false }, Formatter);
                var inactiveMessage = new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = inactiveContent
                };

                Logger.Info("Returning inactive introspection response.");
                return inactiveMessage;
            }

            var response = IntrospectionValidationResult.Claims.ToClaimsDictionary();
            response.Add("active", true);
            response.Add("scope", Scope.Name);

            var content = new ObjectContent<Dictionary<string, object>>(response, Formatter);
            var message = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = content
            };

            Logger.Info("Returning active introspection response.");
            return message;
        }
    }
}