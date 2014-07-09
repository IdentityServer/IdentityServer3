using Thinktecture.IdentityServer.Core.Connect.Models;

namespace Thinktecture.IdentityServer.Core.Services
{
    public interface IRefreshTokenStore : ITransientDataRepository<RefreshToken>
    {
    }
}