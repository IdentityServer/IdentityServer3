/*
 * Copyright (c) Dominick Baier, Brock Allen.  All rights reserved.
 * see license
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
using Thinktecture.IdentityServer.Core.Connect.Models;
using Thinktecture.IdentityServer.Core.Connect.Services;
using Thinktecture.IdentityServer.Core.Logging;
using Thinktecture.IdentityServer.Core.Models;
using Thinktecture.IdentityServer.Core.Services;

namespace Thinktecture.IdentityServer.Core.Connect
{
    public class TokenValidator
    {
        private readonly CoreSettings _settings;
        private readonly IUserService _users;
        private readonly ITokenHandleStore _tokenHandles;
        private readonly ICustomTokenValidator _customValidator;
        private readonly ILog _logger;
        private readonly IClientService _clients;

        public TokenValidator(CoreSettings settings, IUserService users, IClientService clients, ITokenHandleStore tokenHandles, ICustomTokenValidator customValidator)
        {
            _settings = settings;
            _users = users;
            _clients = clients;
            _tokenHandles = tokenHandles;
            _customValidator = customValidator;

            _logger = LogProvider.GetCurrentClassLogger();
        }

        public virtual Task<TokenValidationResult> ValidateIdentityTokenAsync(string token)
        {
            throw new NotImplementedException();
        }

        public virtual async Task<TokenValidationResult> ValidateAccessTokenAsync(string token, string expectedScope = null)
        {
            _logger.Info("Start access token validation");
            _logger.Debug("Token: " + token);

            var result = new TokenValidationResult();

            if (token.Contains("."))
            {
                _logger.InfoFormat("Validating a JWT access token");
                result = await ValidateJwtAccessTokenAsync(token);
            }
            else
            {
                _logger.InfoFormat("Validating a reference access token");
                result = await ValidateReferenceAccessTokenAsync(token);
            }

            if (expectedScope.IsPresent())
            {
                var scope = result.Claims.FirstOrDefault(c => c.Type == Constants.ClaimTypes.Scope && c.Value == expectedScope);
                if (scope == null)
                {
                    _logger.InfoFormat("Checking for expected scope {0} failed", expectedScope);
                    return Invalid(Constants.ProtectedResourceErrors.InsufficientScope);
                }

                _logger.InfoFormat("Checking for expected scope {0} succeeded", expectedScope);
            }

            _logger.Debug("Calling custom token validator");
            var customResult = await _customValidator.ValidateAccessTokenAsync(result, _settings, _clients, _users);

            if (customResult.IsError)
            {
                _logger.Error("Custom validator failed: " + customResult.Error ?? "unknown");
            }
            
            return customResult;
        }

        protected virtual Task<TokenValidationResult> ValidateJwtAccessTokenAsync(string jwt)
        {
            var handler = new JwtSecurityTokenHandler();
            handler.Configuration = new SecurityTokenHandlerConfiguration();
            handler.Configuration.CertificateValidationMode = X509CertificateValidationMode.None;
            handler.Configuration.CertificateValidator = X509CertificateValidator.None;
            
            var parameters = new TokenValidationParameters
            {
                ValidIssuer = _settings.IssuerUri,
                SigningToken = new X509SecurityToken(_settings.SigningCertificate),
                AllowedAudience = string.Format(Constants.AccessTokenAudience, _settings.IssuerUri)
            };

            try
            {
                var id = handler.ValidateToken(jwt, parameters);
                _logger.Info("JWT access token validatio successful");

                return Task.FromResult(new TokenValidationResult
                {
                    Claims = id.Claims,
                    Jwt = jwt
                });
            }
            catch (Exception ex)
            {
                _logger.ErrorException("JWT token validation error", ex);
                return Task.FromResult(Invalid(Constants.ProtectedResourceErrors.InvalidToken));
            }
        }

        protected virtual async Task<TokenValidationResult> ValidateReferenceAccessTokenAsync(string tokenHandle)
        {
            var token = await _tokenHandles.GetAsync(tokenHandle);

            if (token == null)
            {
                _logger.Error("Token handle not found");
                return Invalid(Constants.ProtectedResourceErrors.InvalidToken);
            }

            if (token.Type != Constants.TokenTypes.AccessToken)
            {
                _logger.ErrorFormat("Token handle does not resolve to an access token - but instead to: {1}", tokenHandle, token.Type);
                return Invalid(Constants.ProtectedResourceErrors.InvalidToken);
            }

            if (DateTime.UtcNow > token.CreationTime.AddSeconds(token.Lifetime))
            {
                _logger.Error("Token expired.");
                return Invalid(Constants.ProtectedResourceErrors.ExpiredToken);
            }

            _logger.Info("Reference access token validation successful");

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