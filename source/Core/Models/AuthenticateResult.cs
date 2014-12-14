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

using System;
using System.Linq;
using System.Collections.Generic;
using System.Security.Claims;
using Thinktecture.IdentityServer.Core.Extensions;

namespace Thinktecture.IdentityServer.Core.Models
{
    public class AuthenticateResult
    {
        public ClaimsPrincipal User { get; private set; }
        public string ErrorMessage { get; private set; }
        
        public string PartialSignInRedirectPath { get; private set; }

        public AuthenticateResult(string errorMessage)
        {
            if (errorMessage.IsMissing()) throw new ArgumentNullException("errorMessage");
            ErrorMessage = errorMessage;
        }
        
        internal AuthenticateResult(ClaimsPrincipal user)
        {
            if (user == null) throw new ArgumentNullException("user");

            this.User = IdentityServerPrincipal.CreateFromPrincipal(user, Constants.PrimaryAuthenticationType);
        }

        void Init(string subject, string name,
            IEnumerable<Claim> claims = null,
            string identityProvider = Constants.BuiltInIdentityProvider,
            string authenticationMethod = null,
            string authenticationType = Constants.PrimaryAuthenticationType
        )
        {
            if (String.IsNullOrWhiteSpace(subject)) throw new ArgumentNullException("subject");
            if (String.IsNullOrWhiteSpace(name)) throw new ArgumentNullException("name");
            if (String.IsNullOrWhiteSpace(identityProvider)) throw new ArgumentNullException("identityProvider");

            if (String.IsNullOrWhiteSpace(authenticationMethod))
            {
                if (identityProvider == Constants.BuiltInIdentityProvider)
                {
                    authenticationMethod = Constants.AuthenticationMethods.Password;
                }
                else
                {
                    authenticationMethod = Constants.AuthenticationMethods.External;
                }
            }

            var user = IdentityServerPrincipal.Create(subject, name, authenticationMethod, identityProvider, authenticationType);
            if (claims != null && claims.Any())
            {
                claims = claims.Where(x => !Constants.ExternalAuthenticationType.Contains(x.Type));
                claims = claims.Where(x => !Constants.AuthenticateResultClaimTypes.Contains(x.Type));
                user.Identities.First().AddClaims(claims);
            }

            this.User = user;
        }

        public AuthenticateResult(string subject, string name,
            IEnumerable<Claim> claims = null,
            string identityProvider = Constants.BuiltInIdentityProvider,
            string authenticationMethod = null
        )
        {
            Init(subject, name, claims, identityProvider, authenticationMethod);
        }
        
        public AuthenticateResult(string redirectPath, string subject, string name, 
            IEnumerable<Claim> claims = null,
            string identityProvider = Constants.BuiltInIdentityProvider,
            string authenticationMethod = null
        )
            : this(subject, name, claims, identityProvider, authenticationMethod)
        {
            if (redirectPath.IsMissing()) throw new ArgumentNullException("redirectPath");
            if (!redirectPath.StartsWith("~/") && !redirectPath.StartsWith("/"))
            {
                throw new ArgumentException("redirectPath must start with / or ~/");
            }

            Init(subject, name, claims, identityProvider, authenticationMethod, Constants.PartialSignInAuthenticationType);
            this.PartialSignInRedirectPath = redirectPath;
        }

        public AuthenticateResult(string redirectPath, ExternalIdentity externalId)
        {
            if (redirectPath.IsMissing()) throw new ArgumentNullException("redirectPath");
            if (!redirectPath.StartsWith("~/") && !redirectPath.StartsWith("/"))
            {
                throw new ArgumentException("redirectPath must start with / or ~/");
            }
            if (externalId == null) throw new ArgumentNullException("externalId");

            this.PartialSignInRedirectPath = redirectPath;

            var id = new ClaimsIdentity(externalId.Claims, Constants.PartialSignInAuthenticationType);
            // we're keeping the external provider info for the partial signin so we can re-execute AuthenticateExternalAsync
            // once the user is re-directed back into identityserver from the external redirect
            id.AddClaim(new Claim(Constants.ClaimTypes.ExternalProviderUserId, externalId.ProviderId, ClaimValueTypes.String, externalId.Provider));
            User = new ClaimsPrincipal(id);
        }

        public bool IsError
        {
            get { return ErrorMessage.IsPresent(); }
        }

        public bool IsPartialSignIn
        {
            get
            {
                return !String.IsNullOrWhiteSpace(PartialSignInRedirectPath);
            }
        }
    }
}