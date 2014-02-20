using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using Thinktecture.IdentityServer.Core.Connect.Models;
using Thinktecture.IdentityServer.Core.Services;

namespace Thinktecture.IdentityServer.Core.Connect.Services
{
    public class DefaultClaimsProvider : IClaimsProvider
    {
        public IEnumerable<Claim> GetIdentityTokenClaims(ClaimsPrincipal user, Client client, IEnumerable<string> scopes, ICoreSettings settings, bool includeAllIdentityClaims, IUserService profile)
        {
            List<Claim> claims = new List<Claim>();
            var scopeDetails = settings.GetScopes();
            var allIdentityClaims = new List<string>();
            var alwaysIncludeIdentityClaims = new List<string>();

            foreach (var scope in scopes)
            {
                var scopeDetail = scopeDetails.FirstOrDefault(s => s.Name == scope);

                if (scopeDetail != null)
                {
                    if (scopeDetail.IsOpenIdScope)
                    {
                        foreach (var claim in scopeDetail.Claims)
                        {
                            allIdentityClaims.Add(claim.Name);

                            if (claim.AlwaysIncludeInIdToken)
                            {
                                alwaysIncludeIdentityClaims.Add(claim.Name);
                            }
                        }
                    }
                }
            }

            // if no access token is request, all identity claims go into id token
            if (includeAllIdentityClaims)
            {
                alwaysIncludeIdentityClaims = alwaysIncludeIdentityClaims.Union(allIdentityClaims).ToList();
            }

            // fetch all identity claims that need to go into the id token
            if (alwaysIncludeIdentityClaims.Count > 0)
            {
                claims.AddRange(profile.GetProfileData(user.GetSubject(), allIdentityClaims));
            }

            return claims;
        }


        public IEnumerable<Claim> GetAccessTokenClaims(ClaimsPrincipal user, Client client, IEnumerable<string> scopes, ICoreSettings settings, IUserService _profile)
        {
            return new List<Claim>();
        }
    }
}
