/*
 * Copyright (c) Dominick Baier, Brock Allen.  All rights reserved.
 * see license
 */

namespace Thinktecture.IdentityServer.Core.Models
{
    public class ScopeClaim
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public bool AlwaysIncludeInIdToken { get; set; }

        public ScopeClaim()
        { }

        public ScopeClaim(string name, bool alwaysInclude = false)
        {
            Name = name;
            Description = string.Empty;
            AlwaysIncludeInIdToken = alwaysInclude;
        }
    }
}