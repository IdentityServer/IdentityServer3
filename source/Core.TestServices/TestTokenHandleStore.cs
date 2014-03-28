using System.Collections.Concurrent;
using System.Threading.Tasks;
using Thinktecture.IdentityServer.Core.Connect.Models;
using Thinktecture.IdentityServer.Core.Connect.Services;


namespace Thinktecture.IdentityServer.TestServices
{
    public class TestTokenHandleStore : ITokenHandleStore
    {
        static ConcurrentDictionary<string, Token> _repository = new ConcurrentDictionary<string, Token>();

        public async Task StoreAsync(string key, Token value)
        {
            _repository[key] = value;
        }

        public async Task<Token> GetAsync(string key)
        {
            Token token;
            if (_repository.TryGetValue(key, out token))
            {
                return token;
            }
            return null;
        }

        public async Task RemoveAsync(string key)
        {
            Token token;
            _repository.TryRemove(key, out token);
        }
    }
}
