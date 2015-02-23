﻿/*
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

using System.ComponentModel;
using System.Net.Http;
using System.Threading.Tasks;
using Thinktecture.IdentityServer.Core.Extensions;
using Thinktecture.IdentityServer.Core.Logging;

#pragma warning disable 1591

namespace Thinktecture.IdentityServer.Core.Validation
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class BearerTokenUsageValidator
    {
        private static readonly ILog Logger = LogProvider.GetCurrentClassLogger();

        public async Task<BearerTokenUsageValidationResult> ValidateAsync(HttpRequestMessage request)
        {
            var result = ValidateAuthorizationHeader(request);
            if (result.TokenFound)
            {
                return result;
            }

            if (request.Method == HttpMethod.Post && request.Content.IsFormData())
            {
                result = await ValidatePostBodyAsync(request);
                if (result.TokenFound)
                {
                    return result;
                }
            }

            return new BearerTokenUsageValidationResult();
        }

        public BearerTokenUsageValidationResult ValidateAuthorizationHeader(HttpRequestMessage request)
        {
            var authorizationHeader = request.Headers.Authorization;

            if (authorizationHeader != null &&
                authorizationHeader.Scheme.Equals(Constants.TokenTypes.Bearer) &&
                authorizationHeader.Parameter.IsPresent())
            {
                return new BearerTokenUsageValidationResult
                {
                    TokenFound = true,
                    Token = authorizationHeader.Parameter,
                    UsageType = BearerTokenUsageType.AuthorizationHeader
                };
            }

            return new BearerTokenUsageValidationResult();
        }

        public async Task<BearerTokenUsageValidationResult> ValidatePostBodyAsync(HttpRequestMessage request)
        {
            var form = await request.Content.ReadAsFormDataAsync();

            var token = form.Get("access_token");
            if (token.IsPresent())
            {
                return new BearerTokenUsageValidationResult
                {
                    TokenFound = true,
                    Token = token,
                    UsageType = BearerTokenUsageType.PostBody
                };
            }

            return new BearerTokenUsageValidationResult();
        }
    }
}