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

using IdentityServer3.Core.Extensions;
using IdentityServer3.Core.Logging;
using IdentityServer3.Core.Models;
using IdentityServer3.Core.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

#pragma warning disable 1591

namespace IdentityServer3.Core.Validation
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class ScopeValidator
    {
        private readonly static ILog Logger = LogProvider.GetCurrentClassLogger();
        
        private readonly IScopeStore _store;

        public bool ContainsOpenIdScopes { get; private set; }
        public bool ContainsResourceScopes { get; private set; }
        public bool ContainsOfflineAccessScope { get; set; }

        public List<Scope> RequestedScopes { get; private set; }
        public List<Scope> GrantedScopes { get; private set; }

        public ScopeValidator(IScopeStore store)
        {
            RequestedScopes = new List<Scope>();
            GrantedScopes = new List<Scope>();

            _store = store;
        }

        public static List<string> ParseScopesString(string scopes)
        {
            if (scopes.IsMissing())
            {
                Logger.Warn("Empty scopes.");
                return null;
            }

            scopes = scopes.Trim();
            var parsedScopes = scopes.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).Distinct().ToList();

            if (parsedScopes.Any())
            {
                parsedScopes.Sort();
                return parsedScopes;
            }

            return null;
        }

        public void SetConsentedScopes(IEnumerable<string> consentedScopes)
        {
            consentedScopes = consentedScopes ?? Enumerable.Empty<string>();

            GrantedScopes.RemoveAll(scope => !scope.Required && !consentedScopes.Contains(scope.Name));
        }

        public async Task<bool> AreScopesValidAsync(IEnumerable<string> requestedScopes)
        {
            var availableScopes = await _store.FindScopesAsync(requestedScopes);

            foreach (var requestedScope in requestedScopes)
            {
                var scopeDetail = availableScopes.FirstOrDefault(s => s.Name == requestedScope);

                if (scopeDetail == null)
                {
                    Logger.ErrorFormat("Invalid scope: {0}", requestedScope);
                    return false;
                }

                if (scopeDetail.Enabled == false)
                {
                    Logger.ErrorFormat("Scope disabled: {0}", requestedScope);
                    return false;
                }

                if (scopeDetail.Type == ScopeType.Identity)
                {
                    ContainsOpenIdScopes = true;
                }
                else
                {
                    ContainsResourceScopes = true;
                }

                GrantedScopes.Add(scopeDetail);
            }

            if (requestedScopes.Contains(Constants.StandardScopes.OfflineAccess))
            {
                ContainsOfflineAccessScope = true;
            }

            RequestedScopes.AddRange(GrantedScopes);

            return true;
        }

        public bool AreScopesAllowed(Client client, IEnumerable<string> requestedScopes)
        {
            if (client.AllowAccessToAllScopes)
            {
                return true;
            }

            foreach (var scope in requestedScopes)
            {
                if (!client.AllowedScopes.Contains(scope))
                {
                    Logger.ErrorFormat("Requested scope not allowed: {0}", scope);
                    return false;
                }
            }

            return true;
        }

        public bool IsResponseTypeValid(string responseType)
        {
            var requirement = Constants.ResponseTypeToScopeRequirement[responseType];

            // must include identity scopes
            if (requirement == Constants.ScopeRequirement.Identity)
            {
                if (!ContainsOpenIdScopes)
                {
                    Logger.Error("Requests for id_token response type must include identity scopes");
                    return false;
                }
            }

            // must include identity scopes only
            else if (requirement == Constants.ScopeRequirement.IdentityOnly)
            {
                if (!ContainsOpenIdScopes || ContainsResourceScopes)
                {
                    Logger.Error("Requests for id_token response type only must not include resource scopes");
                    return false;
                }
            }

            // must include resource scopes only
            else if (requirement == Constants.ScopeRequirement.ResourceOnly)
            {
                if (ContainsOpenIdScopes || !ContainsResourceScopes)
                {
                    Logger.Error("Requests for token response type only must include resource scopes, but no identity scopes.");
                    return false;
                }
            }

            return true;
        }
    }
}