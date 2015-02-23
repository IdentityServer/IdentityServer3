﻿/*
 * Copyright 2014, 2015 Dominick Baier, Brock Allen
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *   http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Thinktecture.IdentityModel;
using Thinktecture.IdentityServer.Core.Extensions;
using Thinktecture.IdentityServer.Core.Logging;
using Thinktecture.IdentityServer.Core.Models;
using Thinktecture.IdentityServer.Core.Services;

#pragma warning disable 1591

namespace Thinktecture.IdentityServer.Core.ResponseHandling
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class UserInfoResponseGenerator
    {
        private readonly static ILog Logger = LogProvider.GetCurrentClassLogger();
        private readonly IUserService _users;
        private readonly IScopeStore _scopes;

        public UserInfoResponseGenerator(IUserService users, IScopeStore scopes)
        {
            _users = users;
            _scopes = scopes;
        }

        public async Task<Dictionary<string, object>> ProcessAsync(string subject, IEnumerable<string> scopes)
        {
            Logger.Info("Creating userinfo response");
            var profileData = new Dictionary<string, object>();
            
            var requestedClaimTypes = await GetRequestedClaimTypesAsync(scopes);
            Logger.InfoFormat("Requested claim types: {0}", requestedClaimTypes.ToSpaceSeparatedString());

            var principal = Principal.Create("UserInfo", new Claim("sub", subject));
            var profileClaims = await _users.GetProfileDataAsync(principal, requestedClaimTypes);
            
            if (profileClaims != null)
            {
                profileData = profileClaims.ToClaimsDictionary();
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
            if (scopes == null || !scopes.Any())
            {
                return Enumerable.Empty<string>();
            }

            var scopeString = string.Join(" ", scopes);
            Logger.InfoFormat("Scopes in access token: {0}", scopeString);

            var scopeDetails = await _scopes.FindScopesAsync(scopes);
            var scopeClaims = new List<string>();

            foreach (var scope in scopes)
            {
                var scopeDetail = scopeDetails.FirstOrDefault(s => s.Name == scope);
                
                if (scopeDetail != null)
                {
                    if (scopeDetail.Type == ScopeType.Identity)
                    {
                        scopeClaims.AddRange(scopeDetail.Claims.Select(c => c.Name));
                    }
                }
            }

            return scopeClaims;
        }
    }
}