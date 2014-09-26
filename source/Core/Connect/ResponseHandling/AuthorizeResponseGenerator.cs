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
using System.Security.Claims;
using System.Threading.Tasks;
using Thinktecture.IdentityServer.Core.Connect.Models;
using Thinktecture.IdentityServer.Core.Extensions;
using Thinktecture.IdentityServer.Core.Services;

namespace Thinktecture.IdentityServer.Core.Connect
{
    public class AuthorizeResponseGenerator
    {
        private readonly ITokenService _tokenService;
        private readonly IAuthorizationCodeStore _authorizationCodes;

        public AuthorizeResponseGenerator(ITokenService tokenService, IAuthorizationCodeStore authorizationCodes)
        {
            _tokenService = tokenService;
            _authorizationCodes = authorizationCodes;
        }

        public async Task<AuthorizeResponse> CreateCodeFlowResponseAsync(ValidatedAuthorizeRequest request, ClaimsPrincipal subject)
        {
            var code = new AuthorizationCode
            {
                Client = request.Client,
                Subject = subject,

                IsOpenId = request.IsOpenIdRequest,
                RequestedScopes = request.ValidatedScopes.GrantedScopes,
                RedirectUri = request.RedirectUri,

                WasConsentShown = request.WasConsentShown,
            };

            // store id token and access token and return authorization code
            var id = Guid.NewGuid().ToString("N");
            await _authorizationCodes.StoreAsync(id, code);

            return new AuthorizeResponse
            {
                RedirectUri = request.RedirectUri,
                Code = id,
                State = request.State
            };
        }

        public async Task<AuthorizeResponse> CreateImplicitFlowResponseAsync(ValidatedAuthorizeRequest request)
        {
            string accessTokenValue = null;
            int accessTokenLifetime = 0;

            if (request.IsResourceRequest)
            {
                var accessToken = await _tokenService.CreateAccessTokenAsync(request.Subject, request.Client, request.ValidatedScopes.GrantedScopes, request.Raw);
                accessTokenLifetime = accessToken.Lifetime;

                accessTokenValue = await _tokenService.CreateSecurityTokenAsync(accessToken);
            }

            string jwt = null;
            if (request.IsOpenIdRequest)
            {
                var idToken = await _tokenService.CreateIdentityTokenAsync(request.Subject, request.Client, request.ValidatedScopes.GrantedScopes, !request.AccessTokenRequested, request.Raw, accessTokenValue);
                jwt = await _tokenService.CreateSecurityTokenAsync(idToken);
            }

            return new AuthorizeResponse
            {
                RedirectUri = request.RedirectUri,
                AccessToken = accessTokenValue,
                AccessTokenLifetime = accessTokenLifetime,
                IdentityToken = jwt,
                State = request.State,
                Scope = request.ValidatedScopes.GrantedScopes.ToSpaceSeparatedString()
            };
        }
    }
}