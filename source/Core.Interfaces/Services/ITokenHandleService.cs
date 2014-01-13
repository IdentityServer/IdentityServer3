using Thinktecture.IdentityServer.Core.Models;

namespace Thinktecture.IdentityServer.Core.Services
{
    public interface ITokenHandleService
    {
        string Store(Token token);
        Token Find(string id);
        void Delete(string id);
    }
}
