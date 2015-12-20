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

using IdentityServer3.Core.Models;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;

namespace IdentityServer3.Core.Validation
{
    internal class IntrospectionRequestValidator
    {
        private readonly TokenValidator _tokenValidator;

        public IntrospectionRequestValidator(TokenValidator tokenValidator)
        {
            _tokenValidator = tokenValidator;
        }

        public async Task<IntrospectionRequestValidationResult> ValidateAsync(NameValueCollection parameters, Scope scope)
        {
            var fail = new IntrospectionRequestValidationResult { IsError = true };

            // retrieve required token
            var token = parameters.Get("token");
            if (token == null)
            {
                fail.IsActive = false;
                fail.FailureReason = IntrospectionRequestValidationFailureReason.MissingToken;
                return fail;
            }

            // validate token
            var tokenValidationResult = await _tokenValidator.ValidateAccessTokenAsync(token);

            // invalid or unknown token
            if (tokenValidationResult.IsError)
            {
                fail.IsActive = false;
                fail.FailureReason = IntrospectionRequestValidationFailureReason.InvalidToken;
                fail.Token = token;
                return fail;
            }

            // check expected scope
            var expectedScope = tokenValidationResult.Claims.FirstOrDefault(
                c => c.Type == Constants.ClaimTypes.Scope && c.Value == scope.Name);

            // expected scope not present
            if (expectedScope == null)
            {
                fail.IsActive = false;
                fail.IsError = true;
                fail.FailureReason = IntrospectionRequestValidationFailureReason.InvalidScope;
                fail.Token = token;
                return fail;
            }

            // all is good
            var success = new IntrospectionRequestValidationResult
            {
                IsActive = true,
                IsError = false,
                Token = token,
                Claims = tokenValidationResult.Claims
            };

            return success;
        }
    }
}