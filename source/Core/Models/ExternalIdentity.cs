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
using System.Security.Claims;

namespace IdentityServer3.Core.Models
{
    /// <summary>
    /// Models the identity of a user authenticating from an external identity provider.
    /// </summary>
    public class ExternalIdentity
    {
        /// <summary>
        /// Identifier of the external identity provider.
        /// </summary>
        /// <value>
        /// The provider.
        /// </value>
        public string Provider { get; set; }


        /// <summary>
        /// User's unique identifier provided by the external identity provider.
        /// </summary>
        /// <value>
        /// The provider identifier.
        /// </value>
        public string ProviderId { get; set; }

        /// <summary>
        /// Claims supplied for the user from the external identity provider.
        /// </summary>
        /// <value>
        /// The claims.
        /// </value>
        public IEnumerable<Claim> Claims { get; set; }

        /// <summary>
        /// Creates an ExternalIdentity and determines the Provider and ProviderId from the list of claims.
        /// </summary>
        /// <param name="claims">The claims.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException">claims</exception>
        public static ExternalIdentity FromClaims(IEnumerable<Claim> claims)
        {
            if (claims == null) throw new ArgumentNullException("claims");

            var subClaim = claims.FirstOrDefault(x => x.Type == Constants.ClaimTypes.Subject);
            if (subClaim == null)
            {
                subClaim = claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier);

                if (subClaim == null)
                {
                    return null;
                }
            }

            claims = claims.Except(new[] { subClaim });
            
            return new ExternalIdentity
            {
                Provider = subClaim.Issuer,
                ProviderId = subClaim.Value,
                Claims = claims
            };
        }
    }
}