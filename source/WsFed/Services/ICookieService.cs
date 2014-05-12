using System.Collections.Generic;

namespace Thinktecture.IdentityServer.WsFed.Services
{
    public interface ICookieService
    {
        void AddValue(string value);
        IEnumerable<string> GetValuesAndDeleteCookie();
    }
}