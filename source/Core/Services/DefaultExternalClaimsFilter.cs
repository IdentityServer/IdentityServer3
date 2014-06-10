/*
 * Copyright (c) Dominick Baier, Brock Allen.  All rights reserved.
 * see license
 */
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using Thinktecture.IdentityServer.Core.Models;

namespace Thinktecture.IdentityServer.Core.Services
{
    public class DefaultExternalClaimsFilter : IExternalClaimsFilter
    {
        protected string FacebookProviderName = "Facebook";
        protected string TwitterProviderName = "Twitter";

        public IEnumerable<Claim> Filter(IdentityProvider provider, IEnumerable<Claim> claims)
        {
            claims = NormalizeExternalClaimTypes(claims);

            claims = TransformSocialClaims(provider, claims);

            return claims;
        }

        protected virtual IEnumerable<Claim> NormalizeExternalClaimTypes(IEnumerable<Claim> incomingClaims)
        {
            return Thinktecture.IdentityServer.Core.Plumbing.ClaimMap.Map(incomingClaims);
        }

        protected virtual IEnumerable<Claim> TransformSocialClaims(IdentityProvider provider, IEnumerable<Claim> claims)
        {
            if (provider.Name == FacebookProviderName)
            {
                claims = TransformFacebookClaims(claims);
            }
            else if (provider.Name == TwitterProviderName)
            {
                claims = TransformTwitterClaims(claims);
            }

            return claims;
        }

        protected virtual IEnumerable<Claim> TransformFacebookClaims(IEnumerable<Claim> claims)
        {
            var nameClaim = claims.FirstOrDefault(x => x.Type == "urn:facebook:name");
            if (nameClaim != null)
            {
                var list = claims.ToList();
                list.Remove(nameClaim);
                list.RemoveAll(x => x.Type == Constants.ClaimTypes.Name);
                list.Add(new Claim(Constants.ClaimTypes.Name, nameClaim.Value));
                return list;
            }
            return claims;
        }

        protected virtual IEnumerable<Claim> TransformTwitterClaims(IEnumerable<Claim> claims)
        {
            return claims.Where(x => x.Type != "urn:twitter:userid");
        }
    }
}
