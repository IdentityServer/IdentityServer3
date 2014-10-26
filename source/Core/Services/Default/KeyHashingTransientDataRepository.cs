/*
 * Copyright 2014 Dominick Baier, Brock Allen
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *   http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Thinktecture.IdentityModel;
using Thinktecture.IdentityServer.Core.Models;

namespace Thinktecture.IdentityServer.Core.Services.Default
{
    public class KeyHashingRefreshTokenStore : KeyHashingTransientDataRepository<RefreshToken>, IRefreshTokenStore
    {
        public KeyHashingRefreshTokenStore(IRefreshTokenStore inner)
            : base(inner)
        {
        }
    }
    public class KeyHashingAuthorizationCodeStore : KeyHashingTransientDataRepository<AuthorizationCode>, IAuthorizationCodeStore
    {
        public KeyHashingAuthorizationCodeStore(IAuthorizationCodeStore inner)
            : base(inner)
        {
        }
    }
    public class KeyHashingTokenHandleStore : KeyHashingTransientDataRepository<Token>, ITokenHandleStore
    {
        public KeyHashingTokenHandleStore(ITokenHandleStore inner)
            : base(inner)
        {
        }
    }

    public class KeyHashingTransientDataRepository<T> : ITransientDataRepository<T>
        where T : ITokenMetadata
    {
        readonly HashAlgorithm hash;
        readonly ITransientDataRepository<T> inner;

        public KeyHashingTransientDataRepository(ITransientDataRepository<T> inner)
            : this(Constants.DefaultHashAlgorithm, inner)
        {
        }

        public KeyHashingTransientDataRepository(string hashName, ITransientDataRepository<T> inner)
        {
            if (String.IsNullOrWhiteSpace(hashName)) throw new ArgumentNullException("hashName");
            if (inner == null) throw new ArgumentNullException("inner");

            hash = HashAlgorithm.Create(hashName);
            this.inner = inner;
        }

        protected string Hash(string value)
        {
            if (String.IsNullOrWhiteSpace(value)) throw new ArgumentNullException("inner");

            var bytes = Encoding.UTF8.GetBytes(value);
            var hashedBytes = hash.ComputeHash(bytes);
            var hashedString = Base64Url.Encode(hashedBytes);
            return hashedString;
        }

        public Task StoreAsync(string key, T value)
        {
            return inner.StoreAsync(Hash(key), value);
        }

        public Task<T> GetAsync(string key)
        {
            return inner.GetAsync(Hash(key));
        }

        public Task RemoveAsync(string key)
        {
            return inner.RemoveAsync(Hash(key));
        }

        public Task<IEnumerable<ITokenMetadata>> GetAllAsync(string subject)
        {
            return inner.GetAllAsync(subject);
        }

        public Task RevokeAsync(string subject, string client)
        {
            return inner.RevokeAsync(subject, client);
        }
    }
}
