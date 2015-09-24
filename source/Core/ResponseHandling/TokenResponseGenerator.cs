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

using IdentityServer3.Core.Extensions;
using IdentityServer3.Core.Logging;
using IdentityServer3.Core.Models;
using IdentityServer3.Core.Services;
using IdentityServer3.Core.Validation;
using System;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

#pragma warning disable 1591

namespace IdentityServer3.Core.ResponseHandling
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class TokenResponseGenerator
    {
        private readonly static ILog Logger = LogProvider.GetCurrentClassLogger();

        private readonly ITokenService _tokenService;
        private readonly IRefreshTokenService _refreshTokenService;
        private readonly IScopeStore _scopes;
       
        public TokenResponseGenerator(ITokenService tokenService, IRefreshTokenService refreshTokenService, IScopeStore scopes)
        {
            _tokenService = tokenService;
            _refreshTokenService = refreshTokenService;
            _scopes = scopes;
        }

        public async Task<TokenResponse> ProcessAsync(ValidatedTokenRequest request)
        {
            Logger.Info("Creating token response");

            if (request.GrantType == Constants.GrantTypes.AuthorizationCode)
            {
                return await ProcessAuthorizationCodeRequestAsync(request);
            }

            if (request.GrantType == Constants.GrantTypes.RefreshToken)
            {
                return await ProcessRefreshTokenRequestAsync(request);
            }

            return await ProcessTokenRequestAsync(request);
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
                var tokenRequest = new TokenCreationRequest
                {
                    Subject = request.AuthorizationCode.Subject,
                    Client = request.AuthorizationCode.Client,
                    Scopes = request.AuthorizationCode.RequestedScopes,
                    Nonce = request.AuthorizationCode.Nonce,

                    ValidatedRequest = request
                };

                var idToken = await _tokenService.CreateIdentityTokenAsync(tokenRequest);
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
            string accessTokenString;
            
            if (request.Client.UpdateAccessTokenClaimsOnRefresh)
            {
                var subject = request.RefreshToken.GetOriginalSubject();

                var creationRequest = new TokenCreationRequest
                {
                    Client = request.Client,
                    Subject = subject,
                    ValidatedRequest = request,
                    Scopes = await _scopes.FindScopesAsync(oldAccessToken.Scopes)
                };

                var newAccessToken = await _tokenService.CreateAccessTokenAsync(creationRequest);
                accessTokenString = await _tokenService.CreateSecurityTokenAsync(newAccessToken);
            }
            else
            {
                oldAccessToken.CreationTime = DateTimeOffsetHelper.UtcNow;
                oldAccessToken.Lifetime = request.Client.AccessTokenLifetime;

                accessTokenString = await _tokenService.CreateSecurityTokenAsync(oldAccessToken);
            }

            var handle = await _refreshTokenService.UpdateRefreshTokenAsync(request.RefreshTokenHandle, request.RefreshToken, request.Client);

            return new TokenResponse
                {
                    AccessToken = accessTokenString,
                    AccessTokenLifetime = request.Client.AccessTokenLifetime,
                    RefreshToken = handle
                };
        }

        private async Task<Tuple<string, string>> CreateAccessTokenAsync(ValidatedTokenRequest request)
        {
            TokenCreationRequest tokenRequest;
            bool createRefreshToken;

            if (request.AuthorizationCode != null)
            {
                createRefreshToken = request.AuthorizationCode.RequestedScopes.Select(s => s.Name).Contains(Constants.StandardScopes.OfflineAccess);
                
                tokenRequest = new TokenCreationRequest
                {
                    Subject = request.AuthorizationCode.Subject,
                    Client = request.AuthorizationCode.Client,
                    Scopes = request.AuthorizationCode.RequestedScopes,
                    ValidatedRequest = request
                };
            }
            else
            {
                createRefreshToken = request.ValidatedScopes.ContainsOfflineAccessScope;

                tokenRequest = new TokenCreationRequest
                {
                    Subject = request.Subject,
                    Client = request.Client,
                    Scopes = request.ValidatedScopes.GrantedScopes,
                    ValidatedRequest = request
                };
            }

            Token accessToken = await _tokenService.CreateAccessTokenAsync(tokenRequest);

            string refreshToken = "";
            if (createRefreshToken)
            {
                refreshToken = await _refreshTokenService.CreateRefreshTokenAsync(tokenRequest.Subject, accessToken, request.Client);
            }

            var securityToken = await _tokenService.CreateSecurityTokenAsync(accessToken);
            return Tuple.Create(securityToken, refreshToken);
        }
    }
}