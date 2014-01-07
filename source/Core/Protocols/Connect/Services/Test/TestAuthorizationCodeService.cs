using System;
using System.Collections.Concurrent;
using Thinktecture.IdentityServer.Core.Protocols.Connect.Models;
using Thinktecture.IdentityServer.Core.Protocols.Connect.Services;

namespace Thinktecture.IdentityServer.Core.Protocols.Connect.Services
{
    public class TestAuthorizationCodeService : IAuthorizationCodeService
    {
        static ConcurrentDictionary<string, AuthorizationCode> _repository = new ConcurrentDictionary<string, AuthorizationCode>();

        public string Store(AuthorizationCode code)
        {
            var id = Guid.NewGuid().ToString("N");
            _repository[id] = code;

            return id;
        }

        public AuthorizationCode GetAndDelete(string id)
        {
            AuthorizationCode code;
            if (_repository.TryRemove(id, out code))
            {
                return code;
            }

            return null;
        }
    }
}