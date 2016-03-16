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
using Microsoft.Owin;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace IdentityServer3.Core.Validation
{
    internal class BearerTokenUsageValidator
    {
        public async Task<BearerTokenUsageValidationResult> ValidateAsync(IOwinContext context)
        {
            var result = ValidateAuthorizationHeader(context);
            if (result.TokenFound)
            {
                return result;
            }

            if (context.Request.IsFormData())
            {
                result = await ValidatePostBodyAsync(context);
                if (result.TokenFound)
                {
                    return result;
                }
            }

            return new BearerTokenUsageValidationResult();
        }

        public BearerTokenUsageValidationResult ValidateAuthorizationHeader(IOwinContext context)
        {
            var authorizationHeaders = context.Request.Headers.GetValues("Authorization");
            if (authorizationHeaders != null)
            {
                var header = authorizationHeaders.First().Trim();
                if (header.StartsWith(Constants.AuthenticationSchemes.BearerAuthorizationHeader))
                {
                    var value = header.Substring(Constants.AuthenticationSchemes.BearerAuthorizationHeader.Length).Trim();
                    if (value != null && value.Length > 0)
                    {
                        return new BearerTokenUsageValidationResult
                        {
                            TokenFound = true,
                            Token = value,
                            UsageType = BearerTokenUsageType.AuthorizationHeader
                        };
                    }
                }
            }

            return new BearerTokenUsageValidationResult();
        }

        public async Task<BearerTokenUsageValidationResult> ValidatePostBodyAsync(IOwinContext context)
        {
            var form = await context.ReadRequestFormAsNameValueCollectionAsync();

            var token = form.Get(Constants.AuthenticationSchemes.BearerFormPost);
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