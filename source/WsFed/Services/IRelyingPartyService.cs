using Thinktecture.IdentityServer.WsFed.Models;

namespace Thinktecture.IdentityServer.WsFed.Services
{
    public interface IRelyingPartyService
    {
        RelyingParty GetByRealm(string realm);
    }
}