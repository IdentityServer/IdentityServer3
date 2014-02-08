using System.Collections.Concurrent;
using Thinktecture.IdentityServer.Core.Connect.Models;
using Thinktecture.IdentityServer.Core.Connect.Services;

namespace Thinktecture.IdentityServer.Core.Connect.TestServices
{
    public class TestAuthorizationCodeStore : IAuthorizationCodeStore
    {
        static ConcurrentDictionary<string, AuthorizationCode> _repository = new ConcurrentDictionary<string, AuthorizationCode>();

        public void Store(string key, AuthorizationCode value)
        {
            _repository[key] = value;
        }

        public AuthorizationCode Get(string key)
        {
            AuthorizationCode code;
            if (_repository.TryRemove(key, out code))
            {
                return code;
            }

            return null;
        }

        public void Remove(string key)
        {
            AuthorizationCode val;
            _repository.TryRemove(key, out val);
        }
    }
}