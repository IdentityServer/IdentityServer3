using Thinktecture.IdentityServer.Core.Connect.Models;

namespace Thinktecture.IdentityServer.Core.Connect.Services
{
    public interface IAuthorizationCodeService
    {
        string Store(AuthorizationCode response);
        AuthorizationCode GetAndDelete(string code);
    }
}