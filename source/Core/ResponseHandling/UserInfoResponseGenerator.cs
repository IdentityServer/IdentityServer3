/*
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

using IdentityModel;
using IdentityServer3.Core.Extensions;
using IdentityServer3.Core.Logging;
using IdentityServer3.Core.Models;
using IdentityServer3.Core.Services;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace IdentityServer3.Core.ResponseHandling
{
    internal class UserInfoResponseGenerator
    {
        private readonly static ILog Logger = LogProvider.GetCurrentClassLogger();
        private readonly IUserService _users;
        private readonly IScopeStore _scopes;

        public UserInfoResponseGenerator(IUserService users, IScopeStore scopes)
        {
            _users = users;
            _scopes = scopes;
        }

        public async Task<Dictionary<string, object>> ProcessAsync(string subject, IEnumerable<string> scopes, Client client)
        {
            Logger.Info("Creating userinfo response");
            var profileData = new Dictionary<string, object>();
            
            var requestedClaimTypes = await GetRequestedClaimTypesAsync(scopes);
            var principal = Principal.Create("UserInfo", new Claim("sub", subject));

            IEnumerable<Claim> profileClaims;
            if (requestedClaimTypes.IncludeAllClaims)
            {
                Logger.InfoFormat("Requested claim types: all");

                var context = new ProfileDataRequestContext(
                    principal, 
                    client, 
                    Constants.ProfileDataCallers.UserInfoEndpoint);

                await _users.GetProfileDataAsync(context);
                profileClaims = context.IssuedClaims;
            }
            else
            {
                Logger.InfoFormat("Requested claim types: {0}", requestedClaimTypes.ClaimTypes.ToSpaceSeparatedString());

                var context = new ProfileDataRequestContext(
                    principal,
                    client,
                    Constants.ProfileDataCallers.UserInfoEndpoint,
                    requestedClaimTypes.ClaimTypes);

                await _users.GetProfileDataAsync(context);
                profileClaims = context.IssuedClaims;
            }
            
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

        public async Task<RequestedClaimTypes> GetRequestedClaimTypesAsync(IEnumerable<string> scopes)
        {
            if (scopes == null || !scopes.Any())
            {
                return new RequestedClaimTypes();
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
                        if (scopeDetail.IncludeAllClaimsForUser)
                        {
                            return new RequestedClaimTypes
                            {
                                IncludeAllClaims = true
                            };
                        }

                        scopeClaims.AddRange(scopeDetail.Claims.Select(c => c.Name));
                    }
                }
            }

            return new RequestedClaimTypes(scopeClaims);
        }
    }
}