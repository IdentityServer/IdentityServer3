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
using IdentityServer3.Core.Validation;
using System.Collections.Generic;
using System.Linq;

namespace IdentityServer3.Core.Logging
{
    internal class TokenRequestValidationLog
    {
        public string ClientId { get; set; }
        public string ClientName { get; set; }
        public string GrantType { get; set; }
        public string Scopes { get; set; }

        public string AuthorizationCode { get; set; }
        public string RefreshToken { get; set; }

        public string UserName { get; set; }
        public IEnumerable<string> AuthenticationContextReferenceClasses { get; set; }
        public string Tenant { get; set; }
        public string IdP { get; set; }

        public Dictionary<string, string> Raw { get; set; }

        private static IReadOnlyCollection<string> SensitiveData = new List<string>()
            {
                Constants.TokenRequest.Password,
                Constants.TokenRequest.Assertion,
                Constants.TokenRequest.ClientSecret,
                Constants.TokenRequest.ClientAssertion
            };

        public TokenRequestValidationLog(ValidatedTokenRequest request)
        {
            const string scrubValue = "******";
            
            Raw = request.Raw.ToDictionary();
            
            foreach (var field in SensitiveData.Where(field => Raw.ContainsKey(field)))
            {
                Raw[field] = scrubValue;
            }

            if (request.Client != null)
            {
                ClientId = request.Client.ClientId;
                ClientName = request.Client.ClientName;
            }

            if (request.Scopes != null)
            {
                Scopes = request.Scopes.ToSpaceSeparatedString();
            }

            if (request.SignInMessage != null)
            {
                IdP = request.SignInMessage.IdP;
                Tenant = request.SignInMessage.Tenant;
                AuthenticationContextReferenceClasses = request.SignInMessage.AcrValues;
            }

            GrantType = request.GrantType;
            AuthorizationCode = request.AuthorizationCodeHandle;
            RefreshToken = request.RefreshTokenHandle;
            UserName = request.UserName;
        }
    }
}
