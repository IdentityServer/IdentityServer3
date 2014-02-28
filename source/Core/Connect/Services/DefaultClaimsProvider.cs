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
            List<Claim> outputClaims = new List<Claim>();
            var scopeDetails = settings.GetScopes();

            var additionalClaims = new List<string>();

            // fetch all identity claims that need to go into the id token
            foreach (var scope in scopes)
            {
                var scopeDetail = scopeDetails.FirstOrDefault(s => s.Name == scope);

                if (scopeDetail != null)
                {
                    if (scopeDetail.IsOpenIdScope)
                    {
                        foreach (var scopeClaim in scopeDetail.Claims)
                        {
                            if (includeAllIdentityClaims || scopeClaim.AlwaysIncludeInIdToken)
                            {
                                additionalClaims.Add(scopeClaim.Name);
                            }
                        }
                    }
                }
            }

            if (additionalClaims.Count > 0)
            {
                outputClaims.AddRange(profile.GetProfileData(user.GetSubject(), additionalClaims));
            }

            return outputClaims;
        }

        public IEnumerable<Claim> GetAccessTokenClaims(ClaimsPrincipal user, Client client, IEnumerable<string> scopes, ICoreSettings settings, IUserService _profile)
        {
            var claims = new List<Claim>
            {
                new Claim(Constants.ClaimTypes.ClientId, client.ClientId),
            };

            foreach (var scope in scopes)
            {
                claims.Add(new Claim(Constants.ClaimTypes.Scope, scope));
            }

            if (user != null)
            {
                claims.Add(new Claim(Constants.ClaimTypes.Subject, user.GetSubject()));
            }

            return claims;
        }
    }
}
