using System.Security.Claims;
using System.Threading.Tasks;

namespace Thinktecture.IdentityServer.Core.Connect.Services
{
    public interface IAssertionGrantValidator
    {
        Task<ClaimsPrincipal> ValidateAsync(ValidatedTokenRequest request);
    }
}
