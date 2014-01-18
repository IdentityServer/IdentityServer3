using System;
using System.Collections.Concurrent;
using Thinktecture.IdentityServer.Core.Connect.Models;
using Thinktecture.IdentityServer.Core.Connect.Services;


namespace Thinktecture.IdentityServer.Core.Connect.TestServices
{
    public class TestTokenHandleService : ITokenHandleService
    {
        static ConcurrentDictionary<string, Token> _repository = new ConcurrentDictionary<string, Token>();

        public string Store(Token token)
        {
            var key = Guid.NewGuid().ToString("N");
            _repository[key] = token;

            return key;
        }

        public Token Find(string id)
        {
            Token token;

            if (_repository.TryGetValue(id, out token))
            {
                return token;
            }

            return null;
        }

        public void Delete(string id)
        {
            Token token;
            _repository.TryRemove(id, out token);
        }
    }
}
