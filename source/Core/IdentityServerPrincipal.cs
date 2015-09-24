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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;

namespace IdentityServer3.Core
{
    internal static class IdentityServerPrincipal
    {
        public static ClaimsPrincipal Create(
            string subject,
            string displayName,
            string authenticationMethod = Constants.AuthenticationMethods.Password,
            string idp = Constants.BuiltInIdentityProvider,
            string authenticationType = Constants.PrimaryAuthenticationType,
            long authenticationTime = 0)
        {
            if (String.IsNullOrWhiteSpace(subject)) throw new ArgumentNullException("subject");
            if (String.IsNullOrWhiteSpace(displayName)) throw new ArgumentNullException("displayName");
            if (String.IsNullOrWhiteSpace(authenticationMethod)) throw new ArgumentNullException("authenticationMethod");
            if (String.IsNullOrWhiteSpace(idp)) throw new ArgumentNullException("idp");
            if (String.IsNullOrWhiteSpace(authenticationType)) throw new ArgumentNullException("authenticationType");

            if (authenticationTime <= 0) authenticationTime = DateTimeOffset.UtcNow.ToEpochTime();

            var claims = new List<Claim>
            {
                new Claim(Constants.ClaimTypes.Subject, subject),
                new Claim(Constants.ClaimTypes.Name, displayName),
                new Claim(Constants.ClaimTypes.AuthenticationMethod, authenticationMethod),
                new Claim(Constants.ClaimTypes.IdentityProvider, idp),
                new Claim(Constants.ClaimTypes.AuthenticationTime, authenticationTime.ToString(), ClaimValueTypes.Integer)
            };

            var id = new ClaimsIdentity(claims, authenticationType, Constants.ClaimTypes.Name, Constants.ClaimTypes.Role);
            return new ClaimsPrincipal(id);
        }

        public static ClaimsPrincipal CreateFromPrincipal(ClaimsPrincipal principal, string authenticationType)
        {
            // we require the following claims
            var subject = principal.FindFirst(Constants.ClaimTypes.Subject);
            if (subject == null) throw new InvalidOperationException("sub claim is missing");

            var name = principal.FindFirst(Constants.ClaimTypes.Name);
            if (name == null) throw new InvalidOperationException("name claim is missing");

            var authenticationMethod = principal.FindFirst(Constants.ClaimTypes.AuthenticationMethod);
            if (authenticationMethod == null) throw new InvalidOperationException("amr claim is missing");

            var authenticationTime = principal.FindFirst(Constants.ClaimTypes.AuthenticationTime);
            if (authenticationTime == null) throw new InvalidOperationException("auth_time claim is missing");

            var idp = principal.FindFirst(Constants.ClaimTypes.IdentityProvider);
            if (idp == null) throw new InvalidOperationException("idp claim is missing");

            var id = new ClaimsIdentity(principal.Claims, authenticationType, Constants.ClaimTypes.Name, Constants.ClaimTypes.Role);
            return new ClaimsPrincipal(id);
        }

        public static ClaimsPrincipal FromSubjectId(string subjectId, IEnumerable<Claim> additionalClaims = null)
        {
            var claims = new List<Claim>
            {
                new Claim(Constants.ClaimTypes.Subject, subjectId)
            };

            if (additionalClaims != null)
            {
                claims.AddRange(additionalClaims);
            }

            return Principal.Create(Constants.PrimaryAuthenticationType,
                claims.Distinct(new ClaimComparer()).ToArray());
        }

        public static ClaimsPrincipal FromClaims(IEnumerable<Claim> claims, bool allowMissing = false)
        {
            var sub = claims.FirstOrDefault(c => c.Type == Constants.ClaimTypes.Subject);
            var amr = claims.FirstOrDefault(c => c.Type == Constants.ClaimTypes.AuthenticationMethod);
            var idp = claims.FirstOrDefault(c => c.Type == Constants.ClaimTypes.IdentityProvider);
            var authTime = claims.FirstOrDefault(c => c.Type == Constants.ClaimTypes.AuthenticationTime);

            var id = new ClaimsIdentity(Constants.BuiltInIdentityProvider);

            if (sub != null)
            {
                id.AddClaim(sub);
            }
            else
            {
                if (allowMissing == false)
                {
                    throw new InvalidOperationException("sub claim is missing");
                }
            }

            if (amr != null)
            {
                id.AddClaim(amr);
            }
            else
            {
                if (allowMissing == false)
                {
                    throw new InvalidOperationException("amr claim is missing");
                }
            }

            if (idp != null)
            {
                id.AddClaim(idp);
            }
            else
            {
                if (allowMissing == false)
                {
                    throw new InvalidOperationException("idp claim is missing");
                }
            }

            if (authTime != null)
            {
                id.AddClaim(authTime);
            }
            else
            {
                if (allowMissing == false)
                {
                    throw new InvalidOperationException("auth_time claim is missing");
                }
            }

            return new ClaimsPrincipal(id);
        }
    }
}