using Thinktecture.IdentityServer.Core.Protocols.Connect.Models;

namespace Thinktecture.IdentityServer.Core.Protocols.Connect.Services
{
    public interface IAuthorizationCodeService
    {
        string Store(AuthorizationCode response);
        AuthorizationCode GetAndDelete(string code);
    }
}