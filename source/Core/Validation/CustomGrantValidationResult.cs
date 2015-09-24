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

using IdentityModel;
using IdentityServer3.Core.Extensions;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;

namespace IdentityServer3.Core.Validation
{
    /// <summary>
    /// Models the result of custom grant validation.
    /// </summary>
    public class CustomGrantValidationResult : ValidationResult
    {
        /// <summary>
        /// Gets or sets the principal which represents the result of the authentication.
        /// </summary>
        /// <value>
        /// The principal.
        /// </value>
        public ClaimsPrincipal Principal { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="CustomGrantValidationResult"/> class with an error message.
        /// </summary>
        /// <param name="errorMessage">The error message.</param>
        public CustomGrantValidationResult(string errorMessage)
        {
            Error = errorMessage;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CustomGrantValidationResult"/> class with no error and no user.
        /// </summary>
        public CustomGrantValidationResult()
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="CustomGrantValidationResult"/> class.
        /// </summary>
        /// <param name="subject">The subject claim used to uniquely identifier the user.</param>
        /// <param name="authenticationMethod">The authentication method which describes the custom grant type.</param>
        /// <param name="claims">Additional claims that will be maintained in the principal.</param>
        /// <param name="identityProvider">The identity provider.</param>
        public CustomGrantValidationResult(
            string subject, 
            string authenticationMethod,
            IEnumerable<Claim> claims = null,
            string identityProvider = Constants.BuiltInIdentityProvider)
        {
            var resultClaims = new List<Claim>
            {
                new Claim(Constants.ClaimTypes.Subject, subject),
                new Claim(Constants.ClaimTypes.AuthenticationMethod, authenticationMethod),
                new Claim(Constants.ClaimTypes.IdentityProvider, identityProvider),
                new Claim(Constants.ClaimTypes.AuthenticationTime, DateTimeOffsetHelper.UtcNow.ToEpochTime().ToString(), ClaimValueTypes.Integer)
            };

            if (claims != null && claims.Any())
            {
                resultClaims.AddRange(claims.Where(x => !Constants.OidcProtocolClaimTypes.Contains(x.Type)));
            }

            var id = new ClaimsIdentity(authenticationMethod);
            id.AddClaims(resultClaims.Distinct(new ClaimComparer()));

            Principal = new ClaimsPrincipal(id);

            IsError = false;
        }
    }
}