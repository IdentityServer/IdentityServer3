/*
 * Copyright (c) Dominick Baier, Brock Allen.  All rights reserved.
 * see license
 */

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Thinktecture.IdentityServer.Core.Configuration;
using Thinktecture.IdentityServer.Core.Logging;
using Thinktecture.IdentityServer.Core.Services;

namespace Thinktecture.IdentityServer.Core.Connect
{
    public class UserInfoResponseGenerator
    {
        private readonly static ILog Logger = LogProvider.GetCurrentClassLogger();
        private readonly IUserService _users;
        private readonly IScopeService _scopes;
        private readonly CoreSettings _settings;

        public UserInfoResponseGenerator(IUserService users, IScopeService scopes, CoreSettings settings)
        {
            _users = users;
            _scopes = scopes;
            _settings = settings;
        }

        public async Task<Dictionary<string, object>> ProcessAsync(string subject, IEnumerable<string> scopes)
        {
            Logger.Info("Creating userinfo response");
            var profileData = new Dictionary<string, object>();
            
            var requestedClaimTypes = await GetRequestedClaimTypesAsync(scopes);
            Logger.InfoFormat("Requested claim types: {0}", requestedClaimTypes.ToSpaceSeparatedString());

            var profileClaims = await _users.GetProfileDataAsync(subject, requestedClaimTypes);
            
            if (profileClaims != null)
            {
                foreach (var claim in profileClaims)
                {
                    if (profileData.ContainsKey(claim.Type))
                    {
                        Logger.Warn("Duplicate claim type detected: " + claim.Type);
                    }
                    else
                    {
                        profileData.Add(claim.Type, claim.Value);
                    }
                }

                Logger.InfoFormat("Profile service returned to the following claim types: {0}", profileClaims.Select(c => c.Type).ToSpaceSeparatedString());
            }
            else
            {
                Logger.InfoFormat("Profile service returned no claims (null)");
            }

            return profileData;
        }

        public async Task<IEnumerable<string>> GetRequestedClaimTypesAsync(IEnumerable<string> scopes)
        {
            if (scopes == null || scopes.Count() == 0)
            {
                return Enumerable.Empty<string>();
            }

            var scopeString = string.Join(" ", scopes);
            Logger.InfoFormat("Scopes in access token: {0}", scopeString);

            var scopeDetails = await _scopes.GetScopesAsync();
            var scopeClaims = new List<string>();

            foreach (var scope in scopes)
            {
                var scopeDetail = scopeDetails.FirstOrDefault(s => s.Name == scope);
                
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