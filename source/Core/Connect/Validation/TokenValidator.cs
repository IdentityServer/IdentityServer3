using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Thinktecture.IdentityModel.Extensions;
using Thinktecture.IdentityServer.Core.Connect.Models;
using Thinktecture.IdentityServer.Core.Connect.Services;
using Thinktecture.IdentityServer.Core.Services;

namespace Thinktecture.IdentityServer.Core.Connect
{
    public class TokenValidator
    {
        private readonly ICoreSettings _settings;
        private readonly IUserService _users;
        private readonly ITokenHandleStore _tokenHandles;
        private readonly ILogger _logger;

        public TokenValidator(ICoreSettings settings, IUserService users, ITokenHandleStore tokenHandles, ILogger logger)
        {
            _settings = settings;
            _users = users;
            _tokenHandles = tokenHandles;
            _logger = logger;
        }

        public virtual Task<TokenValidationResult> ValidateIdentityTokenAsync(string token)
        {
            throw new NotImplementedException();
        }

        public virtual async Task<TokenValidationResult> ValidateAccessTokenAsync(string token, string expectedScope = null)
        {
            var result = new TokenValidationResult();

            if (token.Contains("."))
            {
                _logger.Verbose("Validating a JWT access token");
                result = await ValidateJwtAccessTokenAsync(token);
            }
            else
            {
                _logger.Verbose("Validating a reference access token");
                result = await ValidateReferenceAccessTokenAsync(token);
            }

            if (expectedScope.IsPresent())
            {
                var scope = result.Claims.FirstOrDefault(c => c.Type == Constants.ClaimTypes.Scope && c.Value == expectedScope);
                if (scope == null)
                {
                    return Invalid(Constants.ProtectedResourceErrors.InsufficientScope);
                }
            }

            return result;
        }

        protected virtual Task<TokenValidationResult> ValidateJwtAccessTokenAsync(string jwt)
        {
            throw new NotImplementedException();
        }

        protected virtual async Task<TokenValidationResult> ValidateReferenceAccessTokenAsync(string tokenHandle)
        {
            _logger.InformationFormat("Validating token handle: {0}", tokenHandle);
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

            _logger.Information("Validation successful");

            return new TokenValidationResult
            {
                Claims = ReferenceTokenToClaims(token)
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