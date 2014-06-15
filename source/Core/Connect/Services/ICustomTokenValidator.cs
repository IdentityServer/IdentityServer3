using System.Net.Http;
using System.Threading.Tasks;
using Thinktecture.IdentityServer.Core.Models;
using Thinktecture.IdentityServer.Core.Services;

namespace Thinktecture.IdentityServer.Core.Connect.Services
{
    public interface ICustomTokenValidator
    {
        Task<TokenValidationResult> ValidateAccessTokenAsync(TokenValidationResult result, CoreSettings settings, IClientService clients, IUserService users);
        Task<TokenValidationResult> ValidateIdentityTokenAsync(TokenValidationResult result, CoreSettings settings, IClientService clients, IUserService users);
    }
}