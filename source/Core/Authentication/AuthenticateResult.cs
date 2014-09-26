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

namespace Thinktecture.IdentityServer.Core.Authentication
{
    public class AuthenticateResult
    {
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
            if (String.IsNullOrWhiteSpace(subject)) throw new ArgumentNullException("subject");
            if (String.IsNullOrWhiteSpace(name)) throw new ArgumentNullException("name");

            this.Subject = subject;
            this.Name = name;
        }

        // TODO: maybe this should be a PathString?
        public AuthenticateResult(string redirectPath, string subject, string name)
            : this(subject, name)
        {
            if (String.IsNullOrWhiteSpace(redirectPath)) throw new ArgumentNullException("redirectPath");

            this.PartialSignInRedirectPath = new PathString(redirectPath);
        }

        public string ErrorMessage { get; private set; }
        public bool IsError
        {
            get { return !String.IsNullOrWhiteSpace(this.ErrorMessage); }
        }

        public string Subject { get; private set; }
        public string Name { get; private set; }
        
        public PathString PartialSignInRedirectPath { get; private set; }
        public bool IsPartialSignIn
        {
            get
            {
                return PartialSignInRedirectPath.HasValue;
            }
        }
        public ICollection<Claim> RedirectClaims { get; private set; }
    }
}