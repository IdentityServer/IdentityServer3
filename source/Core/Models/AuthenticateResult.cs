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

        public AuthenticateResult(ClaimsPrincipal user)
        {
            if (user == null) throw new ArgumentNullException("user");
            User = IdentityServerPrincipal.CreateFromPrincipal(user,  Constants.PrimaryAuthenticationType);
        }

        public AuthenticateResult(string redirectPath, ClaimsPrincipal user)
        {
            if (redirectPath.IsMissing()) throw new ArgumentNullException("redirectPath");
            if (!redirectPath.StartsWith("~/") && !redirectPath.StartsWith("/"))
            {
                throw new ArgumentException("redirectPath must start with / or ~/");
            }
            if (user == null) throw new ArgumentNullException("user");

            this.PartialSignInRedirectPath = redirectPath;
            User = IdentityServerPrincipal.CreateFromPrincipal(user, Constants.PartialSignInAuthenticationType);
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