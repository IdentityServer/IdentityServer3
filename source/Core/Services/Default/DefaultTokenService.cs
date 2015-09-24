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

using IdentityModel;
using IdentityServer3.Core.Configuration;
using IdentityServer3.Core.Extensions;
using IdentityServer3.Core.Logging;
using IdentityServer3.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace IdentityServer3.Core.Services.Default
{
    /// <summary>
    /// Default token service
    /// </summary>
    public class DefaultTokenService : ITokenService
    {
        /// <summary>
        /// The logger
        /// </summary>
        private readonly static ILog Logger = LogProvider.GetCurrentClassLogger();

        /// <summary>
        /// The identity server options
        /// </summary>
        protected readonly IdentityServerOptions _options;

        /// <summary>
        /// The claims provider
        /// </summary>
        protected readonly IClaimsProvider _claimsProvider;

        /// <summary>
        /// The token handles
        /// </summary>
        protected readonly ITokenHandleStore _tokenHandles;

        /// <summary>
        /// The signing service
        /// </summary>
        protected readonly ITokenSigningService _signingService;

        /// <summary>
        /// The events service
        /// </summary>
        protected readonly IEventService _events;

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultTokenService" /> class.
        /// </summary>
        /// <param name="options">The options.</param>
        /// <param name="claimsProvider">The claims provider.</param>
        /// <param name="tokenHandles">The token handles.</param>
        /// <param name="signingService">The signing service.</param>
        /// <param name="events">The events service.</param>
        public DefaultTokenService(IdentityServerOptions options, IClaimsProvider claimsProvider, ITokenHandleStore tokenHandles, ITokenSigningService signingService, IEventService events)
        {
            _options = options;
            _claimsProvider = claimsProvider;
            _tokenHandles = tokenHandles;
            _signingService = signingService;
            _events = events;
        }

        /// <summary>
        /// Creates an identity token.
        /// </summary>
        /// <param name="request">The token creation request.</param>
        /// <returns>
        /// An identity token
        /// </returns>
        public virtual async Task<Token> CreateIdentityTokenAsync(TokenCreationRequest request)
        {
            Logger.Debug("Creating identity token");
            request.Validate();

            // host provided claims
            var claims = new List<Claim>();

            // if nonce was sent, must be mirrored in id token
            if (request.Nonce.IsPresent())
            {
                claims.Add(new Claim(Constants.ClaimTypes.Nonce, request.Nonce));
            }

            // add iat claim
            claims.Add(new Claim(Constants.ClaimTypes.IssuedAt, DateTimeOffsetHelper.UtcNow.ToEpochTime().ToString(), ClaimValueTypes.Integer));

            // add at_hash claim
            if (request.AccessTokenToHash.IsPresent())
            {
                claims.Add(new Claim(Constants.ClaimTypes.AccessTokenHash, HashAdditionalData(request.AccessTokenToHash)));
            }

            // add c_hash claim
            if (request.AuthorizationCodeToHash.IsPresent())
            {
                claims.Add(new Claim(Constants.ClaimTypes.AuthorizationCodeHash, HashAdditionalData(request.AuthorizationCodeToHash)));
            }

            claims.AddRange(await _claimsProvider.GetIdentityTokenClaimsAsync(
                request.Subject,
                request.Client,
                request.Scopes,
                request.IncludeAllIdentityClaims,
                request.ValidatedRequest));

            var token = new Token(Constants.TokenTypes.IdentityToken)
            {
                Audience = request.Client.ClientId,
                Issuer = _options.IssuerUri,
                Lifetime = request.Client.IdentityTokenLifetime,
                Claims = claims.Distinct(new ClaimComparer()).ToList(),
                Client = request.Client
            };

            return token;
        }

        /// <summary>
        /// Creates an access token.
        /// </summary>
        /// <param name="request">The token creation request.</param>
        /// <returns>
        /// An access token
        /// </returns>
        public virtual async Task<Token> CreateAccessTokenAsync(TokenCreationRequest request)
        {
            Logger.Debug("Creating access token");
            request.Validate();

            var claims = new List<Claim>();
            claims.AddRange(await _claimsProvider.GetAccessTokenClaimsAsync(
                request.Subject,
                request.Client,
                request.Scopes,
                request.ValidatedRequest));

            if (request.Client.IncludeJwtId)
            {
                claims.Add(new Claim(Constants.ClaimTypes.JwtId, CryptoRandom.CreateUniqueId()));
            }

            var token = new Token(Constants.TokenTypes.AccessToken)
            {
                Audience = string.Format(Constants.AccessTokenAudience, _options.IssuerUri.EnsureTrailingSlash()),
                Issuer = _options.IssuerUri,
                Lifetime = request.Client.AccessTokenLifetime,
                Claims = claims.Distinct(new ClaimComparer()).ToList(),
                Client = request.Client
            };

            return token;
        }

        /// <summary>
        /// Creates a serialized and protected security token.
        /// </summary>
        /// <param name="token">The token.</param>
        /// <returns>
        /// A security token in serialized form
        /// </returns>
        /// <exception cref="System.InvalidOperationException">Invalid token type.</exception>
        public virtual async Task<string> CreateSecurityTokenAsync(Token token)
        {
            string tokenResult;

            if (token.Type == Constants.TokenTypes.AccessToken)
            {
                if (token.Client.AccessTokenType == AccessTokenType.Jwt)
                {
                    Logger.Debug("Creating JWT access token");

                    tokenResult = await _signingService.SignTokenAsync(token);
                }
                else
                {
                    Logger.Debug("Creating reference access token");

                    var handle = CryptoRandom.CreateUniqueId();
                    await _tokenHandles.StoreAsync(handle, token);

                    tokenResult = handle;
                }
            }
            else if (token.Type == Constants.TokenTypes.IdentityToken)
            {
                Logger.Debug("Creating JWT identity token");

                tokenResult = await _signingService.SignTokenAsync(token);
            }
            else
            {
                throw new InvalidOperationException("Invalid token type.");
            }

            await _events.RaiseTokenIssuedEventAsync(token);
            return tokenResult;
        }

        /// <summary>
        /// Hashes an additional data (e.g. for c_hash or at_hash).
        /// </summary>
        /// <param name="tokenToHash">The token to hash.</param>
        /// <returns></returns>
        protected virtual string HashAdditionalData(string tokenToHash)
        {
            using (var sha = SHA256.Create())
            {
                var hash = sha.ComputeHash(Encoding.ASCII.GetBytes(tokenToHash));

                var leftPart = new byte[16];
                Array.Copy(hash, leftPart, 16);

                return Base64Url.Encode(leftPart);
            }
        }
    }
}