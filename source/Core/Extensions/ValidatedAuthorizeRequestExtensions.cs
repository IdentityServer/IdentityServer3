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

using System.Collections.Generic;
using System.Linq;
using Thinktecture.IdentityServer.Core.Models;
using Thinktecture.IdentityServer.Core.Validation;
using Thinktecture.IdentityServer.Core.ViewModels;

namespace Thinktecture.IdentityServer.Core.Extensions
{
    static class ValidatedAuthorizeRequestExtensions
    {
        public static IEnumerable<ConsentScopeViewModel> GetIdentityScopes(this ValidatedAuthorizeRequest validatedRequest)
        {
            var requestedScopes = validatedRequest.ValidatedScopes.RequestedScopes.Where(x => x.Type == ScopeType.Identity);
            var consentedScopeNames = validatedRequest.ValidatedScopes.GrantedScopes.Select(x => x.Name);
            return requestedScopes.ToConsentScopeViewModel(consentedScopeNames);
        }

        public static IEnumerable<ConsentScopeViewModel> GetResourceScopes(this ValidatedAuthorizeRequest validatedRequest)
        {
            var requestedScopes = validatedRequest.ValidatedScopes.RequestedScopes.Where(x=> x.Type == ScopeType.Resource);
            var consentedScopeNames = validatedRequest.ValidatedScopes.GrantedScopes.Select(x => x.Name);
            return requestedScopes.ToConsentScopeViewModel(consentedScopeNames);
        }

        public static IEnumerable<ConsentScopeViewModel> ToConsentScopeViewModel(this IEnumerable<Scope> scopes, IEnumerable<string> selected)
        {
            var values =
                from s in scopes
                select new ConsentScopeViewModel
                {
                    Selected = selected.Contains(s.Name),
                    Name = s.Name,
                    DisplayName = s.DisplayName,
                    Description = s.Description,
                    Emphasize = s.Emphasize,
                    Required = s.Required
                };
            return values;
        }
    }
}