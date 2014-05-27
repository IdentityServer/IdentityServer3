/*
 * Copyright (c) Dominick Baier, Brock Allen.  All rights reserved.
 * see license
 */
using System.Collections.Generic;

namespace Thinktecture.IdentityServer.Core.Configuration
{
    public class AuthenticationOptions
    {
        public IEnumerable<LoginPageLink> LoginPageLinks { get; set; }
    }

    public class LoginPageLink
    {
        public string Text { get; set; }
        public string Href { get; set; }
    }
}