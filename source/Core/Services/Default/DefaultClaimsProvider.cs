/*
 * Copyright (c) Dominick Baier, Brock Allen.  All rights reserved.
 * see license
 */

using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Thinktecture.IdentityServer.Core.Extensions;
using Thinktecture.IdentityServer.Core.Logging;
using Thinktecture.IdentityServer.Core.Models;

namespace Thinktecture.IdentityServer.Core.Services
{
    public class DefaultClaimsProvider : IClaimsProvider
    {
        private readonly static ILog Logger = LogProvider.GetCurrentClassLogger();

        private readonly IUserService _users;

        public DefaultClaimsProvider(IUserService users)
        {
            _users = users;
        }

        public virtual async Task<IEnumerable<Claim>> GetIdentityTokenClaimsAsync(ClaimsPrincipal subject, Client client, IEnumerable<Scope> scopes, bool includeAllIdentityClaims, NameValueCollection request)
        {
            Logger.Debug("Getting claims for identity token");

            var outputClaims = new List<Claim>(GetStandardSubjectClaims(subject));
            var additionalClaims = new List<string>();

            // fetch all identity claims that need to go into the id token
            foreach (var scope in scopes)
            {
                if (scope.Type == ScopeType.Identity)
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
                var claims = await _users.GetProfileDataAsync(subject.GetSubjectId(), additionalClaims);
                if (claims != null)
                {
                    outputClaims.AddRange(claims);
                }
            }

            return outputClaims;
        }

        public virtual async Task<IEnumerable<Claim>> GetAccessTokenClaimsAsync(ClaimsPrincipal subject, Client client, IEnumerable<Scope> scopes, NameValueCollection request)
        {
            Logger.Debug("Getting claims for access token");

            var outputClaims = new List<Claim>
            {
                new Claim(Constants.ClaimTypes.ClientId, client.ClientId),
            };

            foreach (var scope in scopes)
            {
                outputClaims.Add(new Claim(Constants.ClaimTypes.Scope, scope.Name));
            }

            if (subject != null)
            {
                outputClaims.AddRange(GetStandardSubjectClaims(subject));

                // fetch all resource claims that need to go into the id token
                var additionalClaims = new List<string>();
                foreach (var scope in scopes)
                {
                    if (scope.Type == ScopeType.Resource)
                    {
                        if (scope.Claims != null)
                        {
                            foreach (var scopeClaim in scope.Claims)
                            {
                                additionalClaims.Add(scopeClaim.Name);
                            }
                        }
                    }
                }

                if (additionalClaims.Count > 0)
                {
                    var claims = await _users.GetProfileDataAsync(subject.GetSubjectId(), additionalClaims.Distinct());
                    if (claims != null)
                    {
                        outputClaims.AddRange(claims);
                    }
                }
            }

            return outputClaims;
        }

        protected virtual IEnumerable<Claim> GetStandardSubjectClaims(ClaimsPrincipal subject)
        {
            var claims = new List<Claim>
            {
                subject.FindFirst(Constants.ClaimTypes.Subject),
                subject.FindFirst(Constants.ClaimTypes.AuthenticationMethod),
                subject.FindFirst(Constants.ClaimTypes.AuthenticationTime),
                subject.FindFirst(Constants.ClaimTypes.IdentityProvider),
            };

            return claims;
        }
    }
}