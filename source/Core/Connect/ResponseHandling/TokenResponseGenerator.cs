/*
 * Copyright (c) Dominick Baier, Brock Allen.  All rights reserved.
 * see license
 */

using System;
using System.Linq;
using System.Threading.Tasks;
using Thinktecture.IdentityServer.Core.Connect.Models;
using Thinktecture.IdentityServer.Core.Extensions;
using Thinktecture.IdentityServer.Core.Logging;
using Thinktecture.IdentityServer.Core.Services;

namespace Thinktecture.IdentityServer.Core.Connect
{
    public class TokenResponseGenerator
    {
        private readonly static ILog Logger = LogProvider.GetCurrentClassLogger();

        private readonly ITokenService _tokenService;
        private readonly IRefreshTokenService _refreshTokenService;

        public TokenResponseGenerator(ITokenService tokenService, IRefreshTokenService refreshTokenService)
        {
            _tokenService = tokenService;
            _refreshTokenService = refreshTokenService;
        }

        public async Task<TokenResponse> ProcessAsync(ValidatedTokenRequest request)
        {
            Logger.Info("Creating token response");

            if (request.GrantType == Constants.GrantTypes.AuthorizationCode)
            {
                return await ProcessAuthorizationCodeRequestAsync(request);
            }
            
            if (request.GrantType == Constants.GrantTypes.ClientCredentials ||
                     request.GrantType == Constants.GrantTypes.Password ||
                     request.Assertion.IsPresent())
            {
                return await ProcessTokenRequestAsync(request);
            }
            
            if (request.GrantType == Constants.GrantTypes.RefreshToken)
            {
                return await ProcessRefreshTokenRequestAsync(request);
            }

            throw new InvalidOperationException("Unknown grant type.");
        }

        private async Task<TokenResponse> ProcessAuthorizationCodeRequestAsync(ValidatedTokenRequest request)
        {
            Logger.Info("Processing authorization code request");

            //////////////////////////
            // access token
            /////////////////////////
            var accessToken = await CreateAccessTokenAsync(request);
            var response = new TokenResponse
            {
                AccessToken = accessToken.Item1,
                AccessTokenLifetime = request.Client.AccessTokenLifetime
            };
            
            //////////////////////////
            // refresh token
            /////////////////////////
            if (accessToken.Item2.IsPresent())
            {
                response.RefreshToken = accessToken.Item2;
            }

            //////////////////////////
            // id token
            /////////////////////////
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
            Logger.Info("Processing token request");

            var accessToken = await CreateAccessTokenAsync(request);
            var response = new TokenResponse
            {
                AccessToken = accessToken.Item1,
                AccessTokenLifetime = request.Client.AccessTokenLifetime
            };

            if (accessToken.Item2.IsPresent())
            {
                response.RefreshToken = accessToken.Item2;
            }

            return response;
        }

        private async Task<TokenResponse> ProcessRefreshTokenRequestAsync(ValidatedTokenRequest request)
        {
            Logger.Info("Processing refresh token request");

            var oldAccessToken = request.RefreshToken.AccessToken;
            oldAccessToken.CreationTime = DateTime.UtcNow;
            oldAccessToken.Lifetime = request.Client.AccessTokenLifetime;

            var newAccessToken = await _tokenService.CreateSecurityTokenAsync(oldAccessToken);
            var handle = await _refreshTokenService.UpdateRefreshTokenAsync(request.RefreshToken, request.Client);

            return new TokenResponse
                {
                    AccessToken = newAccessToken,
                    AccessTokenLifetime = request.Client.AccessTokenLifetime,
                    RefreshToken = handle
                };
        }

        private async Task<Tuple<string, string>> CreateAccessTokenAsync(ValidatedTokenRequest request)
        {
            Token accessToken;
            bool createRefreshToken;

            if (request.AuthorizationCode != null)
            {
                createRefreshToken = request.AuthorizationCode.RequestedScopes.Select(s => s.Name).Contains(Constants.StandardScopes.OfflineAccess);
                accessToken = await _tokenService.CreateAccessTokenAsync(request.AuthorizationCode.Subject, request.AuthorizationCode.Client, request.AuthorizationCode.RequestedScopes, request.Raw);
            }
            else
            {
                createRefreshToken = request.ValidatedScopes.ContainsOfflineAccessScope;
                accessToken = await _tokenService.CreateAccessTokenAsync(request.Subject, request.Client, request.ValidatedScopes.GrantedScopes, request.Raw);
            }

            string refreshToken = "";
            if (createRefreshToken)
            {
                refreshToken = await _refreshTokenService.CreateRefreshTokenAsync(accessToken, request.Client);
            }

            var securityToken = await _tokenService.CreateSecurityTokenAsync(accessToken);
            return Tuple.Create(securityToken, refreshToken);
        }
    }
}