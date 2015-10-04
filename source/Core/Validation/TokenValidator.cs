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
using IdentityServer3.Core.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IdentityModel.Selectors;
using System.IdentityModel.Tokens;
using System.Linq;
using System.Security.Claims;
using System.ServiceModel.Security;
using System.Threading.Tasks;

#pragma warning disable 1591

namespace IdentityServer3.Core.Validation
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class TokenValidator
    {
        private readonly static ILog Logger = LogProvider.GetCurrentClassLogger();
        private readonly IdentityServerOptions _options;
        private readonly ITokenHandleStore _tokenHandles;
        private readonly ICustomTokenValidator _customValidator;
        private readonly IClientStore _clients;

        private readonly TokenValidationLog _log;

        public TokenValidator(IdentityServerOptions options, IClientStore clients, ITokenHandleStore tokenHandles, ICustomTokenValidator customValidator)
        {
            _options = options;
            _clients = clients;
            _tokenHandles = tokenHandles;
            _customValidator = customValidator;

            _log = new TokenValidationLog();
        }

        public virtual async Task<TokenValidationResult> ValidateIdentityTokenAsync(string token, string clientId = null, bool validateLifetime = true)
        {
            Logger.Info("Start identity token validation");

            if (clientId.IsMissing())
            {
                clientId = GetClientIdFromJwt(token);

                if (clientId.IsMissing())
                {
                    Logger.Error("No clientId supplied, can't find id in identity token.");
                    return Invalid(Constants.ProtectedResourceErrors.InvalidToken);
                }
            }

            _log.ClientId = clientId;
            _log.ValidateLifetime = validateLifetime;

            var client = await _clients.FindClientByIdAsync(clientId);
            if (client == null)
            {
                LogError("Unknown or diabled client.");
                return Invalid(Constants.ProtectedResourceErrors.InvalidToken);
            }

            _log.ClientName = client.ClientName;

            var signingKey = new X509SecurityKey(_options.SigningCertificate);
            var result = await ValidateJwtAsync(token, clientId, signingKey, validateLifetime);

            result.Client = client;

            if (result.IsError)
            {
                LogError("Error validating JWT");
                return result;
            }

            _log.Claims = result.Claims.ToClaimsDictionary();

            var customResult = await _customValidator.ValidateIdentityTokenAsync(result);

            if (customResult.IsError)
            {
                LogError("Custom validator failed: " + (customResult.Error ?? "unknown"));
                return customResult;
            }

            _log.Claims = customResult.Claims.ToClaimsDictionary();

            LogSuccess();
            return customResult;
        }

        public virtual async Task<TokenValidationResult> ValidateAccessTokenAsync(string token, string expectedScope = null)
        {
            Logger.Info("Start access token validation");

            _log.ExpectedScope = expectedScope;
            _log.ValidateLifetime = true;

            TokenValidationResult result;

            if (token.Contains("."))
            {
                _log.AccessTokenType = AccessTokenType.Jwt.ToString();
                result = await ValidateJwtAsync(
                    token,
                    string.Format(Constants.AccessTokenAudience, _options.IssuerUri.EnsureTrailingSlash()),
                    new X509SecurityKey(_options.SigningCertificate));
            }
            else
            {
                _log.AccessTokenType = AccessTokenType.Reference.ToString();
                result = await ValidateReferenceAccessTokenAsync(token);
            }

            _log.Claims = result.Claims.ToClaimsDictionary();

            if (result.IsError)
            {
                return result;
            }

            if (expectedScope.IsPresent())
            {
                var scope = result.Claims.FirstOrDefault(c => c.Type == Constants.ClaimTypes.Scope && c.Value == expectedScope);
                if (scope == null)
                {
                    LogError(string.Format("Checking for expected scope {0} failed", expectedScope));
                    return Invalid(Constants.ProtectedResourceErrors.InsufficientScope);
                }
            }

            var customResult = await _customValidator.ValidateAccessTokenAsync(result);

            if (customResult.IsError)
            {
                LogError("Custom validator failed: " + (customResult.Error ?? "unknown"));
                return customResult;
            }

            // add claims again after custom validation
            _log.Claims = customResult.Claims.ToClaimsDictionary();

            LogSuccess();
            return customResult;
        }

        public virtual async Task<TokenValidationResult> ValidateJwtAsync(string jwt, string audience, SecurityKey signingKey, bool validateLifetime = true)
        {
            var handler = new JwtSecurityTokenHandler
            {
                Configuration =
                    new SecurityTokenHandlerConfiguration
                    {
                        CertificateValidationMode = X509CertificateValidationMode.None,
                        CertificateValidator = X509CertificateValidator.None
                    }
            };

            var parameters = new TokenValidationParameters
            {
                ValidIssuer = _options.IssuerUri,
                IssuerSigningKey = signingKey,
                ValidateLifetime = validateLifetime,
                ValidAudience = audience
            };

            try
            {
                SecurityToken jwtToken;
                var id = handler.ValidateToken(jwt, parameters, out jwtToken);

                // if access token contains an ID, log it
                var jwtId = id.FindFirst(Constants.ClaimTypes.JwtId);
                if (jwtId != null)
                {
                    _log.JwtId = jwtId.Value;
                }

                // load the client that belongs to the client_id claim
                Client client = null;
                var clientId = id.FindFirst(Constants.ClaimTypes.ClientId);
                if (clientId != null)
                {
                    client = await _clients.FindClientByIdAsync(clientId.Value);
                    if (client == null)
                    {
                        throw new InvalidOperationException("Client does not exist anymore.");
                    }
                }

                return new TokenValidationResult
                {
                    IsError = false,

                    Claims = id.Claims,
                    Client = client,
                    Jwt = jwt
                };
            }
            catch (Exception ex)
            {
                Logger.ErrorException("JWT token validation error", ex);
                return Invalid(Constants.ProtectedResourceErrors.InvalidToken);
            }
        }

        protected virtual async Task<TokenValidationResult> ValidateReferenceAccessTokenAsync(string tokenHandle)
        {
            _log.TokenHandle = tokenHandle;
            var token = await _tokenHandles.GetAsync(tokenHandle);

            if (token == null)
            {
                LogError("Token handle not found");
                return Invalid(Constants.ProtectedResourceErrors.InvalidToken);
            }

            if (token.Type != Constants.TokenTypes.AccessToken)
            {
                LogError("Token handle does not resolve to an access token - but instead to: " + token.Type);

                await _tokenHandles.RemoveAsync(tokenHandle);
                return Invalid(Constants.ProtectedResourceErrors.InvalidToken);
            }

            if (DateTimeOffsetHelper.UtcNow >= token.CreationTime.AddSeconds(token.Lifetime))
            {
                LogError("Token expired.");

                await _tokenHandles.RemoveAsync(tokenHandle);
                return Invalid(Constants.ProtectedResourceErrors.ExpiredToken);
            }

            return new TokenValidationResult
            {
                IsError = false,

                Client = token.Client,
                Claims = ReferenceTokenToClaims(token),
                ReferenceToken = token,
                ReferenceTokenId = tokenHandle
            };
        }

        protected virtual IEnumerable<Claim> ReferenceTokenToClaims(Token token)
        {
            var claims = new List<Claim>
            {
                new Claim(Constants.ClaimTypes.Audience, token.Audience),
                new Claim(Constants.ClaimTypes.Issuer, token.Issuer),
                new Claim(Constants.ClaimTypes.NotBefore, token.CreationTime.ToEpochTime().ToString()),
                new Claim(Constants.ClaimTypes.Expiration, token.CreationTime.AddSeconds(token.Lifetime).ToEpochTime().ToString())
            };

            claims.AddRange(token.Claims);

            return claims;
        }

        protected virtual string GetClientIdFromJwt(string token)
        {
            var jwt = new JwtSecurityToken(token);
            var clientId = jwt.Audiences.FirstOrDefault();

            return clientId;
        }

        protected virtual TokenValidationResult Invalid(string error)
        {
            return new TokenValidationResult
            {
                IsError = true,
                Error = error
            };
        }

        private void LogError(string message)
        {
            var json = LogSerializer.Serialize(_log);
            Logger.ErrorFormat("{0}\n{1}", message, json);
        }

        private void LogSuccess()
        {
            var json = LogSerializer.Serialize(_log);
            Logger.InfoFormat("{0}\n{1}", "Token validation success", json);
        }
    }
}
