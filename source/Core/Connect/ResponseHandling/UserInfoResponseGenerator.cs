/*
 * Copyright (c) Dominick Baier, Brock Allen.  All rights reserved.
 * see license
 */

using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Thinktecture.IdentityServer.Core.Connect.Models;
using Thinktecture.IdentityServer.Core.Services;

namespace Thinktecture.IdentityServer.Core.Connect
{
    public class UserInfoResponseGenerator
    {
        private readonly IUserService _users;
        private readonly ICoreSettings _settings;
        private readonly ILogger _logger;

        public UserInfoResponseGenerator(IUserService users, ICoreSettings settings, ILogger logger)
        {
            _users = users;
            _settings = settings;
            _logger = logger;
        }

        public async Task<Dictionary<string, object>> ProcessAsync(IEnumerable<Claim> claims)
        {
            var profileData = new Dictionary<string, object>();
            
            var requestedClaimTypes = await GetRequestedClaimTypesAsync(claims);
            _logger.InformationFormat("Requested claim types: {0}", requestedClaimTypes.ToSpaceSeparatedString());

            var profileClaims = await _users.GetProfileDataAsync(
                    claims.First(c => c.Type == Constants.ClaimTypes.Subject).Value, requestedClaimTypes);
            
            if (profileClaims != null)
            {
                foreach (var claim in profileClaims)
                {
                    profileData.Add(claim.Type, claim.Value);
                }

                _logger.InformationFormat("Profile service returned to the following claim types: {0}", claims.Select(c => c.Type).ToSpaceSeparatedString());
            }
            else
            {
                _logger.InformationFormat("Profile service returned no claims (null)");
            }

            return profileData;
        }

        public async Task<IEnumerable<string>> GetRequestedClaimTypesAsync(IEnumerable<Claim> claims)
        {
            var scopeClaims = new List<string>();

            var scopes = claims.ToList().FindAll(c => c.Type == Constants.ClaimTypes.Scope);
            if (scopes == null)
            {
                _logger.Warning("No scopes found in access token. aborting.");
                return scopeClaims;
            }

            var scopeString = string.Join(" ", scopes.Select(s => s.Value).ToArray());
            _logger.InformationFormat("Scopes in access token: {0}", scopeString);

            var scopeDetails = await _settings.GetScopesAsync();
            
            foreach (var scope in scopes)
            {
                var scopeDetail = scopeDetails.FirstOrDefault(s => s.Name == scope.Value);
                
                if (scopeDetail != null)
                {
                    if (scopeDetail.IsOpenIdScope)
                    {
                        scopeClaims.AddRange(scopeDetail.Claims.Select(c => c.Name));
                    }
                }
            }

            return scopeClaims;
        }
    }
}