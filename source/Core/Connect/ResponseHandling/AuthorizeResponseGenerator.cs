/*
 * Copyright (c) Dominick Baier, Brock Allen.  All rights reserved.
 * see license
 */

using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Thinktecture.IdentityServer.Core.Configuration;
using Thinktecture.IdentityServer.Core.Connect.Models;
using Thinktecture.IdentityServer.Core.Connect.Services;
using Thinktecture.IdentityServer.Core.Extensions;

namespace Thinktecture.IdentityServer.Core.Connect
{
    public class AuthorizeResponseGenerator
    {
        private readonly ITokenService _tokenService;
        private readonly IAuthorizationCodeStore _authorizationCodes;
        private readonly ITokenHandleStore _tokenHandles;
        private readonly CoreSettings _settings;

        public AuthorizeResponseGenerator(ITokenService tokenService, IAuthorizationCodeStore authorizationCodes, ITokenHandleStore tokenHandles, CoreSettings settings)
        {
            _tokenService = tokenService;
            _authorizationCodes = authorizationCodes;
            _tokenHandles = tokenHandles;
            _settings = settings;
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
                RefreshTokenLifetime = request.Client.RefreshTokenLifetime
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