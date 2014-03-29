/*
 * Copyright (c) Dominick Baier, Brock Allen.  All rights reserved.
 * see license
 */

using System.Collections.Generic;
using System.Linq;

namespace Thinktecture.IdentityServer.Core.Connect.Models
{
    public class UserConsent
    {
        public string Button { get; set; }
        public string[] Scopes { get; set; }
        public bool RememberConsent { get; set; }

        public bool WasConsentGranted
        {
            get
            {
                return Button == "yes";
            }
        }

        public IEnumerable<string> ScopedConsented
        {
            get
            {
                if (WasConsentGranted && Scopes != null)
                {
                    return Scopes;
                }

                return Enumerable.Empty<string>();
            }
        }
    }
}
