using System.IdentityModel.Tokens;
using System.Threading.Tasks;
using Thinktecture.IdentityServer.Core.Connect.Models;

namespace Thinktecture.IdentityServer.Core.Services
{
    public interface ITokenSigningService
    {
        Task<string> SignTokenAsync(Token token);
    }
}