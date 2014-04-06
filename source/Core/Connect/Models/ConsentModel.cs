/*
 * Copyright (c) Dominick Baier, Brock Allen.  All rights reserved.
 * see license
 */
using System;
using System.Collections.Specialized;
using System.Linq;

namespace Thinktecture.IdentityServer.Core.Connect.Models
{
    public class ConsentModel
    {
        public ConsentModel(ValidatedAuthorizeRequest validatedRequest, NameValueCollection requestParameters)
        {
            var requestedScopes = validatedRequest.ValidatedScopes.RequestedScopes;
            var consentedScopeNames = validatedRequest.ValidatedScopes.GrantedScopes.Select(x => x.Name);

            var idScopes =
                from s in requestedScopes
                where s.IsOpenIdScope
                select new
                {
                    selected = consentedScopeNames.Contains(s.Name),
                    s.Name,
                    s.DisplayName,
                    s.Description,
                    s.Emphasize,
                    s.Required
                };
            var appScopes =
                from s in requestedScopes
                where !s.IsOpenIdScope
                select new
                {
                    selected = consentedScopeNames.Contains(s.Name),
                    s.Name,
                    s.DisplayName,
                    s.Description,
                    s.Emphasize,
                    s.Required
                };

            postUrl = "consent?" + requestParameters.ToQueryString();
            client = validatedRequest.Client.ClientName;
            clientUrl = validatedRequest.Client.ClientUri;
            clientLogo = validatedRequest.Client.LogoUri;
            identityScopes = idScopes.ToArray();
            applicationScopes = appScopes.ToArray();
            allowRememberConsent = validatedRequest.Client.AllowRememberConsent;
        }

        public string postUrl { get; set; }
        public string client { get; set; }
        public string clientUrl { get; set; }
        public Uri clientLogo { get; set; }
        public object[] identityScopes { get; set; }
        public object[] applicationScopes { get; set; }
        public bool allowRememberConsent { get; set; }
    }
}