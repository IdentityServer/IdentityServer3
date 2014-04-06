using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Thinktecture.IdentityModel.Extensions;
using Thinktecture.IdentityServer.Core.Connect.Models;
using Thinktecture.IdentityServer.Core.Connect.Services;
using Thinktecture.IdentityServer.Core.Services;
using System.Linq;

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
                result = await ValidateJwtAccessTokenAsync(token);
            }
            else
            {
                result = await ValidateReferenceAccessTokenAsync(token);
            }

            if (expectedScope.IsPresent())
            {
                var scope = result.Claims.FirstOrDefault(c => c.Type == Constants.ClaimTypes.Scope && c.Value == expectedScope);
                if (scope == null)
                {
                    return Invalid("missing_scope");
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
            var token = await _tokenHandles.GetAsync(tokenHandle);

            if (token == null)
            {
                return Invalid("invalid_token");
            }

            if (token.Type != Constants.TokenTypes.AccessToken)
            {
                return Invalid("invalid_token");
            }

            if (DateTime.UtcNow > token.CreationTime.AddSeconds(token.Lifetime))
            {
                return Invalid("expired_token");
            }

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