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
using IdentityServer3.Core.Extensions;
using IdentityServer3.Core.Logging;
using IdentityServer3.Core.Models;
using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace IdentityServer3.Core.Results
{
    internal class AuthorizeRedirectResult : IHttpActionResult
    {
        private readonly static ILog Logger = LogProvider.GetCurrentClassLogger();

        private readonly AuthorizeResponse _response;
        private readonly IdentityServerOptions _options;

        public AuthorizeRedirectResult(AuthorizeResponse response, IdentityServerOptions options)
        {
            _response = response;
            _options = options;
        }

        public Task<HttpResponseMessage> ExecuteAsync(System.Threading.CancellationToken cancellationToken)
        {
            return Task.FromResult(Execute());
        }

        HttpResponseMessage Execute()
        {
            var responseMessage = new HttpResponseMessage(HttpStatusCode.Redirect);
            var url = _response.RedirectUri;

            var query = _response.ToNameValueCollection().ToQueryString();

            if (_response.Request.ResponseMode == Constants.ResponseModes.Query)
            {
                url = url.AddQueryString(query);
            }
            else
            {
                url = url.AddHashFragment(query);
            }

            responseMessage.Headers.Location = new Uri(url);

            if (_response.IsError)
            {
                Logger.Info("Redirecting to: " + url);
            }
            else
            {
                Logger.Info("Redirecting to: " + _response.RedirectUri);
            }

            return responseMessage;
        }
    }
}