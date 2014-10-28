/*
 * Copyright 2014 Dominick Baier, Brock Allen
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

using System;
using System.Collections.Generic;
using System.Linq;
using Thinktecture.IdentityServer.Core.Extensions;
using Thinktecture.IdentityServer.Core.Logging;
using Thinktecture.IdentityServer.Core.Models;

namespace Thinktecture.IdentityServer.Core.Validation
{
    public class ScopeValidator
    {
        private readonly static ILog Logger = LogProvider.GetCurrentClassLogger();

        public bool ContainsOpenIdScopes { get; private set; }
        public bool ContainsResourceScopes { get; private set; }
        public bool ContainsOfflineAccessScope { get; set; }

        public List<Scope> RequestedScopes { get; private set; }
        public List<Scope> GrantedScopes { get; private set; }

        public ScopeValidator()
        {
            RequestedScopes = new List<Scope>();
            GrantedScopes = new List<Scope>();
        }

        public void SetConsentedScopes(IEnumerable<string> consentedScopes)
        {
            consentedScopes = consentedScopes ?? Enumerable.Empty<string>();

            GrantedScopes.RemoveAll(scope => !scope.Required && !consentedScopes.Contains(scope.Name));
        }

        public bool AreScopesValid(IEnumerable<string> requestedScopes, IEnumerable<Scope> availableScopes)
        {
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

        public List<string> ParseScopes(string scopes)
        {
            if (scopes.IsMissing())
            {
                return null;
            }

            Logger.InfoFormat("scopes: {0}", scopes);

            scopes = scopes.Trim();
            var parsedScopes = scopes.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).Distinct().ToList();

            if (parsedScopes.Count > 0)
            {
                parsedScopes.Sort();
                return parsedScopes;
            }

            return null;
        }

        public bool AreScopesAllowed(Client client, IEnumerable<string> requestedScopes)
        {
            if (client.ScopeRestrictions == null || client.ScopeRestrictions.Count == 0)
            {
                Logger.Info("All scopes allowed for client");
                return true;
            }

            Logger.Info("Allowed scopes for client client: " + client.ScopeRestrictions.ToSpaceSeparatedString());

            foreach (var scope in requestedScopes)
            {
                if (!client.ScopeRestrictions.Contains(scope))
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


            //if (responseType == Constants.ResponseTypes.IdToken)
            //{
            //    // must include identity scopes, but no resource scopes
            //    if (!ContainsOpenIdScopes || ContainsResourceScopes)
            //    {
            //        Logger.Error("Requests for id_token or id_token token response types must include identity scopes, but no resource scopes");
            //        return false;
            //    }

            //}
            //else if (responseType == Constants.ResponseTypes.IdTokenToken)
            //{
            //    // must include identity scopes
            //    if (!ContainsOpenIdScopes)
            //    {
            //        Logger.Error("Requests for id_token response type must include identity scopes");
            //        return false;
            //    }
            //}
            //else if (responseType == Constants.ResponseTypes.Token)
            //{
            //    // must include resource scopes, but no identity scopes
            //    if (ContainsOpenIdScopes || !ContainsResourceScopes)
            //    {
            //        Logger.Error("Requests for token response type must include resource scopes, but no identity scopes.");
            //        return false;
            //    }
            //}

            return true;
        }
    }
}