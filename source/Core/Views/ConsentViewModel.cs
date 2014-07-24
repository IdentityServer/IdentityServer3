/*
 * Copyright (c) Dominick Baier, Brock Allen.  All rights reserved.
 * see license
 */

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Thinktecture.IdentityServer.Core.Configuration;

namespace Thinktecture.IdentityServer.Core.Views
{
    public class ConsentViewModel : ErrorViewModel
    {
        public string LoginWithDifferentAccountUrl { get; set; }
        public string LogoutUrl { get; set; }
        public string ConsentUrl { get; set; }
        public string ClientName { get; set; }
        public string ClientUrl { get; set; }
        public string ClientLogoUrl { get; set; }
        public bool AllowRememberConsent { get; set; }
        public IEnumerable<ConsentScopeViewModel> IdentityScopes { get; set; }
        public IEnumerable<ConsentScopeViewModel> ApplicationScopes { get; set; }
    }
}
