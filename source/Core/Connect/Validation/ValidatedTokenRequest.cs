/*
 * Copyright (c) Dominick Baier, Brock Allen.  All rights reserved.
 * see license
 */

using System.Collections.Generic;
using System.Collections.Specialized;
using System.Security.Claims;
using Thinktecture.IdentityServer.Core.Configuration;
using Thinktecture.IdentityServer.Core.Connect.Models;
using Thinktecture.IdentityServer.Core.Models;

namespace Thinktecture.IdentityServer.Core.Connect
{
    public class ValidatedTokenRequest
    {
        public NameValueCollection Raw { get; set; }
        public ClaimsPrincipal Subject { get; set; }

        public CoreSettings Settings { get; set; }
        public Client Client { get; set; }
        public string GrantType { get; set; }
        public AuthorizationCode AuthorizationCode { get; set; }
        public IEnumerable<string> Scopes { get; set; }
        public ScopeValidator ValidatedScopes { get; set; }
        public string UserName { get; set; }
        public string Assertion { get; set; }
    }
}
