/*
 * Copyright (c) Dominick Baier, Brock Allen.  All rights reserved.
 * see license
 */

using System;
using System.Threading.Tasks;
using Thinktecture.IdentityServer.Core.Configuration;
using Thinktecture.IdentityServer.Core.Connect.Models;
using Thinktecture.IdentityServer.Core.Connect.Services;
using Thinktecture.IdentityServer.Core.Logging;

namespace Thinktecture.IdentityServer.Core.Connect
{
    public class TokenResponseGenerator
    {
        private readonly static ILog Logger = LogProvider.GetCurrentClassLogger();
        private readonly CoreSettings _settings;
        private readonly ITokenService _tokenService;
        private readonly ITokenHandleStore _tokenHandles;

        public TokenResponseGenerator(ITokenService tokenService, ITokenHandleStore tokenHandles, CoreSettings settings, IAuthorizationCodeStore codes)
        {
            _settings = settings;
            _tokenService = tokenService;
            _tokenHandles = tokenHandles;
        }

        public async Task<TokenResponse> ProcessAsync(ValidatedTokenRequest request)
        {
            Logger.Info("Creating token response");

            if (request.GrantType == Constants.GrantTypes.AuthorizationCode)
            {
                return await ProcessAuthorizationCodeRequestAsync(request);
            }
            else if (request.GrantType == Constants.GrantTypes.ClientCredentials ||
                     request.GrantType == Constants.GrantTypes.Password ||
                     request.Assertion.IsPresent())
            {
                return await ProcessTokenRequestAsync(request);
            }

            throw new InvalidOperationException("Unknown grant type.");
        }

        private async Task<TokenResponse> ProcessAuthorizationCodeRequestAsync(ValidatedTokenRequest request)
        {
            var response = new TokenResponse
            {
                AccessToken = await CreateAccessTokenAsync(request),
                AccessTokenLifetime = request.Client.AccessTokenLifetime
            };

            if (request.AuthorizationCode.IsOpenId)
            {
                var idToken = await _tokenService.CreateIdentityTokenAsync(request.AuthorizationCode.Subject, request.AuthorizationCode.Client, request.AuthorizationCode.RequestedScopes, false, request.Raw);
                var jwt = await _tokenService.CreateSecurityTokenAsync(idToken);    
                response.IdentityToken = jwt;
            }

            return response;
        }

        private async Task<TokenResponse> ProcessTokenRequestAsync(ValidatedTokenRequest request)
        {
            var response = new TokenResponse
            {
                AccessToken = await CreateAccessTokenAsync(request),
                AccessTokenLifetime = request.Client.AccessTokenLifetime
            };

            return response;
        }

        private async Task<string> CreateAccessTokenAsync(ValidatedTokenRequest request)
        {
            Token accessToken;
            if (request.AuthorizationCode != null)
            {
                accessToken = await _tokenService.CreateAccessTokenAsync(request.AuthorizationCode.Subject, request.AuthorizationCode.Client, request.AuthorizationCode.RequestedScopes, request.Raw);
            }
            else
            {
                accessToken = await _tokenService.CreateAccessTokenAsync(request.Subject, request.Client, request.ValidatedScopes.GrantedScopes, request.Raw);
            }

            return await _tokenService.CreateSecurityTokenAsync(accessToken);
        }
    }
}