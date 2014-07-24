/*
 * Copyright (c) Dominick Baier, Brock Allen.  All rights reserved.
 * see license
 */

using System.Collections.Generic;
using Thinktecture.IdentityServer.Core.Configuration;

namespace Thinktecture.IdentityServer.Core.Views
{
    public class LoginViewModel : ErrorViewModel
    {
        public string LoginUrl { get; set; }
        public string Username { get; set; }
        public IEnumerable<LoginPageLink> ExternalProviders { get; set; }
        public IEnumerable<LoginPageLink> AdditionalLinks { get; set; }
    }
}
