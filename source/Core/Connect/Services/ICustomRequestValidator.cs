using Thinktecture.IdentityServer.Core.Services;

namespace Thinktecture.IdentityServer.Core.Connect.Services
{
    public interface ICustomRequestValidator
    {
        ValidationResult ValidateAuthorizeRequest(ValidatedAuthorizeRequest request, IUserService profile);
        ValidationResult ValidateTokenRequest(ValidatedTokenRequest request, IUserService profile);
    }
}