/*
 * Copyright (c) Dominick Baier, Brock Allen.  All rights reserved.
 * see license
 */

namespace Thinktecture.IdentityServer.Core.Configuration
{
    public class InternalConfiguration
    {
        public IDataProtector DataProtector { get; set; }
        public string LoginPageUrl { get; set; }
    }
}
