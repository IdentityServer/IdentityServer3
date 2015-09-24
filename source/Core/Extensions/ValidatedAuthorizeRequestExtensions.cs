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

using IdentityServer3.Core.Models;
using IdentityServer3.Core.Services;
using IdentityServer3.Core.Validation;
using IdentityServer3.Core.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;

namespace IdentityServer3.Core.Extensions
{
    internal static class ValidatedAuthorizeRequestExtensions
    {
        public static IEnumerable<ConsentScopeViewModel> GetIdentityScopes(this ValidatedAuthorizeRequest validatedRequest, ILocalizationService localizationService)
        {
            var requestedScopes = validatedRequest.ValidatedScopes.RequestedScopes.Where(x => x.Type == ScopeType.Identity);
            var consentedScopeNames = validatedRequest.ValidatedScopes.GrantedScopes.Select(x => x.Name);
            return requestedScopes.ToConsentScopeViewModel(consentedScopeNames, localizationService);
        }

        public static IEnumerable<ConsentScopeViewModel> GetResourceScopes(this ValidatedAuthorizeRequest validatedRequest, ILocalizationService localizationService)
        {
            var requestedScopes = validatedRequest.ValidatedScopes.RequestedScopes.Where(x=> x.Type == ScopeType.Resource);
            var consentedScopeNames = validatedRequest.ValidatedScopes.GrantedScopes.Select(x => x.Name);
            return requestedScopes.ToConsentScopeViewModel(consentedScopeNames, localizationService);
        }

        public static IEnumerable<ConsentScopeViewModel> ToConsentScopeViewModel(this IEnumerable<Scope> scopes, IEnumerable<string> selected, ILocalizationService localizationService)
        {
            var values =
                from s in scopes
                select new ConsentScopeViewModel
                {
                    Selected = selected.Contains(s.Name),
                    Name = s.Name,
                    DisplayName = s.DisplayName ?? localizationService.GetScopeDisplayName(s.Name),
                    Description = s.Description ?? localizationService.GetScopeDescription(s.Name),
                    Emphasize = s.Emphasize,
                    Required = s.Required
                };
            return values;
        }

        internal static bool HasIdpAcrValue(this ValidatedAuthorizeRequest request)
        {
            if (request == null) throw new ArgumentNullException("request");

            return request.AuthenticationContextReferenceClasses.Any(x => x.StartsWith(Constants.KnownAcrValues.HomeRealm));
        }
    }
}