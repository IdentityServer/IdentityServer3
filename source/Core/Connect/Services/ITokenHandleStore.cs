using Thinktecture.IdentityServer.Core.Connect.Models;

namespace Thinktecture.IdentityServer.Core.Connect.Services
{
    public interface ITokenHandleStore : ITransientDataRepository<Token>
    {
    }
}
