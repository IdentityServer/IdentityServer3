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

using Microsoft.Owin;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using Thinktecture.IdentityServer.Core.Plumbing;
using Thinktecture.IdentityServer.Core.Extensions;

namespace Thinktecture.IdentityServer.Core.Authentication
{
    public class AuthenticateResult
    {
        public ClaimsPrincipal User { get; set; }
        public string ErrorMessage { get; private set; }
        
        public PathString PartialSignInRedirectPath { get; private set; }
        public ICollection<Claim> RedirectClaims { get; private set; }
        
        protected AuthenticateResult()
        {
            this.RedirectClaims = new HashSet<Claim>();
        }

        public AuthenticateResult(string errorMessage)
            : this()
        {
            if (String.IsNullOrWhiteSpace(errorMessage)) throw new ArgumentNullException("errorMessage");

            this.ErrorMessage = errorMessage;
        }

        public AuthenticateResult(string subject, string name)
            : this()
        {
            if (subject.IsMissing()) throw new ArgumentNullException("subject");
            if (name.IsMissing()) throw new ArgumentNullException("name");

            User = IdentityServerPrincipal.Create(
                subject,
                name,
                Constants.AuthenticationMethods.Password,
                Constants.BuiltInIdentityProvider);
        }

        public AuthenticateResult(string subject, string name, string authenticationMethod, string identityProvider)
            : this()
        {
            if (subject.IsMissing()) throw new ArgumentNullException("subject");
            if (name.IsMissing()) throw new ArgumentNullException("name");

            User = IdentityServerPrincipal.Create(
                subject,
                name,
                authenticationMethod,
                identityProvider);
        }

        public AuthenticateResult(string redirectPath, string subject, string name)
            : this(subject, name)
        {
            if (redirectPath.IsMissing()) throw new ArgumentNullException("redirectPath");
            this.PartialSignInRedirectPath = new PathString(redirectPath);
        }

        public AuthenticateResult(string redirectPath, string subject, string name, string authenticationMethod, string identityProvider)
            : this(subject, name, authenticationMethod, identityProvider)
        {
            if (redirectPath.IsMissing()) throw new ArgumentNullException("redirectPath");
            this.PartialSignInRedirectPath = new PathString(redirectPath);
        }
        
        public bool IsError
        {
            get { return ErrorMessage.IsPresent(); }
        }

        public bool IsPartialSignIn
        {
            get
            {
                return PartialSignInRedirectPath.HasValue;
            }
        }
    }
}