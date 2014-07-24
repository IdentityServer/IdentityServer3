/*
 * Copyright (c) Dominick Baier, Brock Allen.  All rights reserved.
 * see license
 */

using System.Collections.Generic;
using System.Linq;
using Thinktecture.IdentityServer.Core.Connect;
using Thinktecture.IdentityServer.Core.Models;
using Thinktecture.IdentityServer.Core.Views;

namespace Thinktecture.IdentityServer.Core.Extensions
{
    static class ValidatedAuthorizeRequestExtensions
    {
        public static IEnumerable<ConsentScopeViewModel> GetIdentityScopes(this ValidatedAuthorizeRequest validatedRequest)
        {
            var requestedScopes = validatedRequest.ValidatedScopes.RequestedScopes.Where(x => x.IsOpenIdScope);
            var consentedScopeNames = validatedRequest.ValidatedScopes.GrantedScopes.Select(x => x.Name);
            return requestedScopes.ToConsentScopeViewModel(consentedScopeNames);
        }

        public static IEnumerable<ConsentScopeViewModel> GetApplicationScopes(this ValidatedAuthorizeRequest validatedRequest)
        {
            var requestedScopes = validatedRequest.ValidatedScopes.RequestedScopes.Where(x=>!x.IsOpenIdScope);
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
