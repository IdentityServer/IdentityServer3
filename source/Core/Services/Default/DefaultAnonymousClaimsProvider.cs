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
namespace IdentityServer3.Core.Services.Default
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Security.Claims;

    using IdentityServer3.Core.Configuration.Hosting;
    using IdentityServer3.Core.Models;

    /// <summary>
    /// Default anonymous claims provider
    /// </summary>
    public class DefaultAnonymousClaimsProvider : IAnonymousClaimsProvider
    {
        private readonly LastAnonymousIdentifierCookie _anonymousIdentifierCookie;

        public DefaultAnonymousClaimsProvider(LastAnonymousIdentifierCookie anonymousIdentifierCookie)
        {
            _anonymousIdentifierCookie = anonymousIdentifierCookie;
        }

        public IEnumerable<Claim> GetAnonymousClaims(Client client, IEnumerable<Scope> scopes)
        {
            var lastIdentifier = _anonymousIdentifierCookie.GetValue();
            if (string.IsNullOrEmpty(lastIdentifier))
            {
                lastIdentifier = _anonymousIdentifierCookie.CreateNew();
            }
           
            var claims = new List<Claim>
                             {
                                 new Claim(Constants.ClaimTypes.AuthenticationMethod, Constants.Authentication.AnonymousAuthenticationType),
                                 new Claim("did", lastIdentifier),
                                 new Claim(Constants.ClaimTypes.Subject, lastIdentifier),
                                 new Claim(Constants.ClaimTypes.ClientId, client.ClientId),
                             };

            claims.AddRange(scopes.Select(scope => new Claim(Constants.ClaimTypes.Scope, scope.Name)));

            return claims;
        }
    }
}