/*
 * Copyright 2014 Dominick Baier, Brock Allen
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
using System.Security.Claims;
using System.Linq;

namespace Thinktecture.IdentityServer.Core.Validation
{
    public class CustomGrantValidationResult
    {
        public ClaimsPrincipal Principal { get; set; }
        public string ErrorMessage { get; set; }

        public CustomGrantValidationResult(string errorMessage)
        {
            ErrorMessage = errorMessage;
        }

        public CustomGrantValidationResult(
            string subject, 
            string authenticationMethod,
            IEnumerable<Claim> claims = null,
            string identityProvider = Constants.BuiltInIdentityProvider)
        {
            var id = new ClaimsIdentity("CustomGrant");
            id.AddClaim(new Claim(Constants.ClaimTypes.Subject, subject));
            id.AddClaim(new Claim(Constants.ClaimTypes.AuthenticationMethod, authenticationMethod));
            id.AddClaim(new Claim(Constants.ClaimTypes.IdentityProvider, identityProvider));

            if (claims != null && claims.Any())
            {
                id.AddClaims(claims);
            }

            Principal = new ClaimsPrincipal(id);
        }
    }
}