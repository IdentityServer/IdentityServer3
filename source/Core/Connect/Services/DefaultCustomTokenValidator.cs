using System;
using System.Threading.Tasks;
using Thinktecture.IdentityServer.Core.Configuration;
using Thinktecture.IdentityServer.Core.Services;

namespace Thinktecture.IdentityServer.Core.Connect.Services
{
    public class DefaultCustomTokenValidator : ICustomTokenValidator
    {
        public Task<TokenValidationResult> ValidateAccessTokenAsync(TokenValidationResult result, CoreSettings settings, IClientService clients, IUserService users)
        {
            return Task.FromResult(result);
        }

        public Task<TokenValidationResult> ValidateIdentityTokenAsync(TokenValidationResult result, CoreSettings settings, IClientService clients, IUserService users)
        {
            throw new NotImplementedException();
        }
    }
}