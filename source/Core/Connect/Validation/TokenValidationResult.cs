/*
 * Copyright (c) Dominick Baier, Brock Allen.  All rights reserved.
 * see license
 */
using System.Collections.Generic;
using System.Security.Claims;
using Thinktecture.IdentityServer.Core.Connect.Models;

namespace Thinktecture.IdentityServer.Core.Connect
{
    public class TokenValidationResult
    {
        public IEnumerable<Claim> Claims { get; set; }
        public string Jwt { get; set; }
        public Token ReferenceToken { get; set; }

        public string Error { get; set; }
        public bool IsError { get; set; }
    }
}