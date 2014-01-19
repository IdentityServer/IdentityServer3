using System;
using System.Net.Http.Headers;
using Thinktecture.IdentityServer.Core.Connect.Services;

namespace Thinktecture.IdentityServer.Core.Connect
{
    public class UserInfoRequestValidator
    {
        private ITokenHandleService _handles;

        public ValidatedUserInfoRequest ValidatedRequest { get; protected set; }
        
        public UserInfoRequestValidator(ITokenHandleService handles)
        {
            _handles = handles;
        }

        public ValidationResult ValidateRequest(AuthenticationHeaderValue authorizationHeader)
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

            return ValidateRequest(authorizationHeader.Parameter);
        }

        public ValidationResult ValidateRequest(string tokenHandle)
        {
            var token = _handles.Find(tokenHandle);

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