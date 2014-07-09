using Thinktecture.IdentityServer.Core.Connect.Models;

namespace Thinktecture.IdentityServer.Core.Connect.Services
{
    public interface IRefreshTokenStore : ITransientDataRepository<RefreshToken>
    {
    }
}