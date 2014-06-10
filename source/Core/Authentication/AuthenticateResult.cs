/*
 * Copyright (c) Dominick Baier, Brock Allen.  All rights reserved.
 * see license
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