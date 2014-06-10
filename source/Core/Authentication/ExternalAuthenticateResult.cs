/*
 * Copyright (c) Dominick Baier, Brock Allen.  All rights reserved.
 * see license
 */
using System;

namespace Thinktecture.IdentityServer.Core.Authentication
{
    public class ExternalAuthenticateResult : AuthenticateResult
    {
        public ExternalAuthenticateResult(string errorMessage)
            : base(errorMessage)
        {
        }

        public ExternalAuthenticateResult(string provider, string subject, string name)
            : base(subject, name)
        {
            if (String.IsNullOrWhiteSpace(provider)) throw new ArgumentNullException("provider");

            this.Provider = provider;
        }

        public ExternalAuthenticateResult(string redirectPath, string provider, string subject, string name)
            : base(redirectPath, subject, name)
        {
            if (String.IsNullOrWhiteSpace(provider)) throw new ArgumentNullException("provider");

            this.Provider = provider;
        }

        public string Provider { get; private set; }
    }
}