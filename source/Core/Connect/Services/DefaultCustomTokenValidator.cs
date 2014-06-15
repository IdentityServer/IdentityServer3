using System;
using System.Threading.Tasks;

namespace Thinktecture.IdentityServer.Core.Connect.Services
{
    public class DefaultCustomTokenValidator : ICustomTokenValidator
    {
        public Task<TokenValidationResult> ValidateAccessTokenAsync(TokenValidationResult result, Core.Models.CoreSettings settings, Core.Services.IClientService clients, Core.Services.IUserService users)
        {
            return Task.FromResult(result);
        }

        public Task<TokenValidationResult> ValidateIdentityTokenAsync(TokenValidationResult result, Core.Models.CoreSettings settings, Core.Services.IClientService clients, Core.Services.IUserService users)
        {
            throw new NotImplementedException();
        }
    }
}