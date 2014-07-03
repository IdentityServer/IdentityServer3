/*
 * Copyright (c) Dominick Baier, Brock Allen.  All rights reserved.
 * see license
 */

using System.Collections.Generic;
using System.Collections.Specialized;
using System.Security.Claims;
using System.Threading.Tasks;
using Thinktecture.IdentityServer.Core.Configuration;
using Thinktecture.IdentityServer.Core.Logging;
using Thinktecture.IdentityServer.Core.Models;
using Thinktecture.IdentityServer.Core.Services;

namespace Thinktecture.IdentityServer.Core.Connect.Services
{
    public class DefaultClaimsProvider : IClaimsProvider
    {
        private readonly static ILog Logger = LogProvider.GetCurrentClassLogger();

        public virtual async Task<IEnumerable<Claim>> GetIdentityTokenClaimsAsync(ClaimsPrincipal subject, Client client, IEnumerable<Scope> scopes, CoreSettings settings, bool includeAllIdentityClaims, IUserService users, NameValueCollection request)
        {
            Logger.Debug("Getting claims for identity token");

            List<Claim> outputClaims = new List<Claim>(GetStandardSubjectClaims(subject));
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
                var claims = await users.GetProfileDataAsync(subject.GetSubjectId(), additionalClaims);
                if (claims != null)
                {
                    outputClaims.AddRange(claims);
                }
            }

            return outputClaims;
        }

        public virtual Task<IEnumerable<Claim>> GetAccessTokenClaimsAsync(ClaimsPrincipal subject, Client client, IEnumerable<Scope> scopes, CoreSettings settings, IUserService users, NameValueCollection request)
        {
            Logger.Debug("Getting claims for access token");

            var claims = new List<Claim>
            {
                new Claim(Constants.ClaimTypes.ClientId, client.ClientId),
            };

            foreach (var scope in scopes)
            {
                claims.Add(new Claim(Constants.ClaimTypes.Scope, scope.Name));
            }

            if (subject != null)
            {
                claims.AddRange(GetStandardSubjectClaims(subject));
            }

            return Task.FromResult<IEnumerable<Claim>>(claims);
        }

        protected virtual IEnumerable<Claim> GetStandardSubjectClaims(ClaimsPrincipal subject)
        {
            var claims = new List<Claim>
            {
                subject.FindFirst(Constants.ClaimTypes.Subject),
                subject.FindFirst(Constants.ClaimTypes.AuthenticationMethod),
                subject.FindFirst(Constants.ClaimTypes.AuthenticationTime),
                subject.FindFirst(Constants.ClaimTypes.IdentityProvider)
            };

            return claims;
        }
    }
}