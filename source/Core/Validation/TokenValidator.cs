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
using System.Collections.Generic;
using System.IdentityModel.Selectors;
using System.IdentityModel.Tokens;
using System.Linq;
using System.Security.Claims;
using System.ServiceModel.Security;
using System.Threading.Tasks;
using Thinktecture.IdentityModel.Extensions;
using Thinktecture.IdentityServer.Core.Configuration;
using Thinktecture.IdentityServer.Core.Extensions;
using Thinktecture.IdentityServer.Core.Logging;
using Thinktecture.IdentityServer.Core.Models;
using Thinktecture.IdentityServer.Core.Services;

namespace Thinktecture.IdentityServer.Core.Validation
{
    public class TokenValidator
    {
        private readonly static ILog Logger = LogProvider.GetCurrentClassLogger();
        private readonly IdentityServerOptions _options;
        private readonly ITokenHandleStore _tokenHandles;
        private readonly ICustomTokenValidator _customValidator;
        private readonly IClientStore _clients;

        public TokenValidator(IdentityServerOptions options, IUserService users, IClientStore clients, ITokenHandleStore tokenHandles, ICustomTokenValidator customValidator)
        {
            _options = options;
            _clients = clients;
            _tokenHandles = tokenHandles;
            _customValidator = customValidator;
        }

        public virtual async Task<TokenValidationResult> ValidateIdentityTokenAsync(string token, string clientId = null, bool validateLifetime = true)
        {
            if (clientId.IsMissing())
            {
                clientId = GetClientIdFromJwt(token);

                if (clientId.IsMissing())
                {
                    return Invalid(Constants.ProtectedResourceErrors.InvalidToken);
                }
            }

            var client = await _clients.FindClientByIdAsync(clientId);
            if (client == null)
            {
                return Invalid(Constants.ProtectedResourceErrors.InvalidToken);
            }

            SecurityKey signingKey;
            if (client.IdentityTokenSigningKeyType == SigningKeyTypes.ClientSecret)
            {
                signingKey = new InMemorySymmetricSecurityKey(Convert.FromBase64String(client.ClientSecret));
            }
            else
            {
                signingKey = new X509SecurityKey(_options.SigningCertificate);
            }

            var result = await ValidateJwtAsync(token, clientId, signingKey, validateLifetime);
            result.Client = client;

            if (result.IsError)
            {
                return result;
            }

            Logger.Debug("Calling custom token validator");
            var customResult = await _customValidator.ValidateIdentityTokenAsync(result);

            if (customResult.IsError)
            {
                Logger.Error("Custom validator failed: " + customResult.Error ?? "unknown");
            }

            return customResult;
        }

        public virtual async Task<TokenValidationResult> ValidateAccessTokenAsync(string token, string expectedScope = null)
        {
            Logger.Info("Start access token validation");
            Logger.Debug("Token: " + token);

            TokenValidationResult result;

            if (token.Contains("."))
            {
                Logger.InfoFormat("Validating a JWT access token");
                result = await ValidateJwtAsync(
                    token, 
                    string.Format(Constants.AccessTokenAudience, _options.IssuerUri.EnsureTrailingSlash()),
                    new X509SecurityKey(_options.SigningCertificate));
            }
            else
            {
                Logger.InfoFormat("Validating a reference access token");
                result = await ValidateReferenceAccessTokenAsync(token);
            }

            if (result.IsError)
            {
                return result;
            }

            if (expectedScope.IsPresent())
            {
                var scope = result.Claims.FirstOrDefault(c => c.Type == Constants.ClaimTypes.Scope && c.Value == expectedScope);
                if (scope == null)
                {
                    Logger.InfoFormat("Checking for expected scope {0} failed", expectedScope);
                    return Invalid(Constants.ProtectedResourceErrors.InsufficientScope);
                }

                Logger.InfoFormat("Checking for expected scope {0} succeeded", expectedScope);
            }

            Logger.Debug("Calling custom token validator");
            var customResult = await _customValidator.ValidateAccessTokenAsync(result);

            if (customResult.IsError)
            {
                Logger.Error("Custom validator failed: " + customResult.Error ?? "unknown");
            }
            
            return customResult;
        }

        public virtual Task<TokenValidationResult> ValidateJwtAsync(string jwt, string audience, SecurityKey signingKey, bool validateLifetime = true)
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
                Logger.Info("JWT access token validation successful");

                return Task.FromResult(new TokenValidationResult
                {
                    Claims = id.Claims,
                    Jwt = jwt
                });
            }
            catch (Exception ex)
            {
                Logger.ErrorException("JWT token validation error", ex);
                return Task.FromResult(Invalid(Constants.ProtectedResourceErrors.InvalidToken));
            }
        }

        protected virtual async Task<TokenValidationResult> ValidateReferenceAccessTokenAsync(string tokenHandle)
        {
            var token = await _tokenHandles.GetAsync(tokenHandle);

            if (token == null)
            {
                Logger.Error("Token handle not found");
                return Invalid(Constants.ProtectedResourceErrors.InvalidToken);
            }

            if (token.Type != Constants.TokenTypes.AccessToken)
            {
                Logger.ErrorFormat("Token handle does not resolve to an access token - but instead to: {1}", tokenHandle, token.Type);
                return Invalid(Constants.ProtectedResourceErrors.InvalidToken);
            }

            if (DateTime.UtcNow > token.CreationTime.AddSeconds(token.Lifetime))
            {
                Logger.Error("Token expired.");
                return Invalid(Constants.ProtectedResourceErrors.ExpiredToken);
            }

            Logger.Info("Reference access token validation successful");

            return new TokenValidationResult
            {
                Claims = ReferenceTokenToClaims(token),
                ReferenceToken = token
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
    }
}