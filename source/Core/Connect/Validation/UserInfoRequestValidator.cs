using System;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Thinktecture.IdentityServer.Core.Connect.Services;

namespace Thinktecture.IdentityServer.Core.Connect
{
    public class UserInfoRequestValidator
    {
        private ITokenHandleStore _handles;

        public ValidatedUserInfoRequest ValidatedRequest { get; protected set; }
        
        public UserInfoRequestValidator(ITokenHandleStore handles)
        {
            _handles = handles;
        }

        public async Task<ValidationResult> ValidateRequestAsync(AuthenticationHeaderValue authorizationHeader)
        {
            if (authorizationHeader == null ||
                !authorizationHeader.Scheme.Equals(Constants.TokenTypes.Bearer) ||
                authorizationHeader.Parameter.IsMissing())
            {
                return new ValidationResult
                {
                    Error = Constants.UserInfoErrors.InvalidRequest
                };
            }

            return await ValidateRequestAsync(authorizationHeader.Parameter);
        }

        public async Task<ValidationResult> ValidateRequestAsync(string tokenHandle)
        {
            var token = await _handles.GetAsync(tokenHandle);

            if (token == null)
            {
                return new ValidationResult
                {
                    Error = Constants.UserInfoErrors.InvalidToken
                };
            }

            if (token.Type != Constants.TokenTypes.AccessToken)
            {
                return new ValidationResult
                {
                    Error = Constants.UserInfoErrors.InvalidToken
                };
            }

            if (DateTime.UtcNow > token.CreationTime.AddSeconds(token.Lifetime))
            {
                return new ValidationResult
                {
                    Error = Constants.UserInfoErrors.InvalidToken
                };
            }

            ValidatedRequest = new ValidatedUserInfoRequest
            {
                AccessToken = token
            };

            return new ValidationResult
            {
                IsError = false
            };
        }
    }
}