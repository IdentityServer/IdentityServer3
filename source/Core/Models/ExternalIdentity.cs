/*
 * Copyright (c) Dominick Baier, Brock Allen.  All rights reserved.
 * see license
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;

namespace Thinktecture.IdentityServer.Core.Models
{
    public class ExternalIdentity
    {
        public IdentityProvider Provider { get; set; }
        public string ProviderId { get; set; }
        public IEnumerable<Claim> Claims { get; set; }

        public static ExternalIdentity FromClaims(IEnumerable<Claim> claims)
        {
            if (claims == null) throw new ArgumentNullException("claims");

            var subClaim = claims.FirstOrDefault(x => x.Type == Constants.ClaimTypes.Subject);
            if (subClaim == null)
            {
                subClaim = claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier);

                if (subClaim == null)
                {
                    return null;
                }
            }

            claims = claims.Except(new Claim[] { subClaim });
            
            return new ExternalIdentity
            {
                Provider = new IdentityProvider { Name = subClaim.Issuer },
                ProviderId = subClaim.Value,
                Claims = claims
            };
        }
    }
}