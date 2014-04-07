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
    public class Token
    {
        public string Audience { get; set; }
        public string Issuer { get; set; }
        public DateTime CreationTime { get; set; }
        public int Lifetime { get; set; }
        public string Type { get; set; }
        public Client Client { get; set; }

        public List<Claim> Claims { get; set; }

        public Token(string tokenType)
        {
            Type = tokenType;
            CreationTime = DateTime.UtcNow;
        }
    }
}
