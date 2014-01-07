

using Thinktecture.IdentityServer.Core.Protocols.Connect.Models;
namespace Thinktecture.IdentityServer.Core.Protocols.Connect.Services
{
    public interface ITokenHandleService
    {
        string Store(Token token);
        Token Find(string id);
        void Delete(string id);
    }
}
