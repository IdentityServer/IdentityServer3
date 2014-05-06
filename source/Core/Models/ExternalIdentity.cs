/*
 * Copyright (c) Dominick Baier, Brock Allen.  All rights reserved.
 * see license
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Thinktecture.IdentityServer.Core.Models
{
    public class ExternalIdentity
    {
        public IdentityProvider Provider { get; set; }
        public string ProviderId { get; set; }
        public IEnumerable<Claim> Claims { get; set; }
    }
}