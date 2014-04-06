using System;
using System.Threading.Tasks;
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

        public virtual async Task<TokenValidationResult> ValidateAccessTokenAsync(string token)
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
                Token = token
            };
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