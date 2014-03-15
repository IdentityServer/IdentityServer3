namespace Thinktecture.IdentityServer.Core.Connect.Services
{
    public class DefaultCustomRequestValidator : ICustomRequestValidator
    {
        public ValidationResult ValidateAuthorizeRequest(ValidatedAuthorizeRequest request, Core.Services.IUserService profile)
        {
            return new ValidationResult
            {
                IsError = false
            };
        }

        public ValidationResult ValidateTokenRequest(ValidatedTokenRequest request, Core.Services.IUserService profile)
        {
            return new ValidationResult
            {
                IsError = false
            };
        }
    }
}