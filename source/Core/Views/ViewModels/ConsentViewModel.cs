/*
 * Copyright (c) Dominick Baier, Brock Allen.  All rights reserved.
 * see license
 */

using System.Collections.Generic;

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
        public bool RememberConsent { get; set; }
        public IEnumerable<ConsentScopeViewModel> IdentityScopes { get; set; }
        public IEnumerable<ConsentScopeViewModel> ApplicationScopes { get; set; }
    }
}
