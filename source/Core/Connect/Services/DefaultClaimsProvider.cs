using System.Collections.Generic;
using System.Collections.Specialized;
using System.Security.Claims;
using System.Threading.Tasks;
using Thinktecture.IdentityServer.Core.Connect.Models;
using Thinktecture.IdentityServer.Core.Services;

namespace Thinktecture.IdentityServer.Core.Connect.Services
{
    public class DefaultClaimsProvider : IClaimsProvider
    {
        public async Task<IEnumerable<Claim>> GetIdentityTokenClaimsAsync(ClaimsPrincipal subject, Client client, IEnumerable<Scope> scopes, ICoreSettings settings, bool includeAllIdentityClaims, IUserService profile, NameValueCollection request)
        {
            List<Claim> outputClaims = new List<Claim>(subject.Claims);
            var additionalClaims = new List<string>();

            // fetch all identity claims that need to go into the id token
            foreach (var scope in scopes)
            {
                if (scope.IsOpenIdScope)
                {
                    foreach (var scopeClaim in scope.Claims)
                    {
                        if (includeAllIdentityClaims || scopeClaim.AlwaysIncludeInIdToken)
                        {
                            additionalClaims.Add(scopeClaim.Name);
                        }
                    }
                }
            }

            if (additionalClaims.Count > 0)
            {
                var claims = await profile.GetProfileDataAsync(subject.GetSubjectId(), additionalClaims);
                if (claims != null)
                {
                    outputClaims.AddRange(claims);
                }
            }

            return outputClaims;
        }

        public async Task<IEnumerable<Claim>> GetAccessTokenClaimsAsync(ClaimsPrincipal user, Client client, IEnumerable<Scope> scopes, ICoreSettings settings, IUserService _profile, NameValueCollection request)
        {
            var claims = new List<Claim>
            {
                new Claim(Constants.ClaimTypes.ClientId, client.ClientId),
            };

            foreach (var scope in scopes)
            {
                claims.Add(new Claim(Constants.ClaimTypes.Scope, scope.Name));
            }

            if (user != null)
            {
                claims.AddRange(user.Claims);
            }

            return claims;
        }
    }
}