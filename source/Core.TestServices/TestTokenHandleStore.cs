using System.Collections.Concurrent;
using Thinktecture.IdentityServer.Core.Connect.Models;
using Thinktecture.IdentityServer.Core.Connect.Services;


namespace Thinktecture.IdentityServer.TestServices
{
    public class TestTokenHandleStore : ITokenHandleStore
    {
        static ConcurrentDictionary<string, Token> _repository = new ConcurrentDictionary<string, Token>();

        public void Store(string key, Token value)
        {
            _repository[key] = value;
        }

        public Token Get(string key)
        {
            Token token;
            if (_repository.TryGetValue(key, out token))
            {
                return token;
            }
            return null;
        }

        public void Remove(string key)
        {
            Token token;
            _repository.TryRemove(key, out token);
        }
    }
}
