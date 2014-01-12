using Thinktecture.IdentityServer.Core.Connect.Models;

namespace Thinktecture.IdentityServer.Core.Connect.Services
{
    public interface ITokenHandleService
    {
        string Store(Token token);
        Token Find(string id);
        void Delete(string id);
    }
}
