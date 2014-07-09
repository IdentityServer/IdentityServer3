/*
 * Copyright (c) Dominick Baier, Brock Allen.  All rights reserved.
 * see license
 */

using System;
using System.Collections.Generic;
using System.Security.Claims;
using Thinktecture.IdentityServer.Core.Models;

namespace Thinktecture.IdentityServer.Core.Connect.Models
{
    public class AuthorizationCode
    {
        public DateTime CreationTime { get; set; }

        public Client Client { get; set; }
        public ClaimsPrincipal Subject { get; set; }
    
        public bool IsOpenId { get; set; }
        public IEnumerable<Scope> RequestedScopes { get; set; }
        public Uri RedirectUri { get; set; }

        public bool WasConsentShown { get; set; }

        public AuthorizationCode()
        {
            CreationTime = DateTime.UtcNow;
        }
    }
}