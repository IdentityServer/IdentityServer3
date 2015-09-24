/*
 * Copyright 2014, 2015 Dominick Baier, Brock Allen
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

using IdentityServer3.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IdentityServer3.Core.Services.Default
{
    internal class TokenMetadataPermissionsStoreAdapter : IPermissionsStore
    {
        readonly Func<string, Task<IEnumerable<ITokenMetadata>>> get;
        readonly Func<string, string, Task> delete;

        public TokenMetadataPermissionsStoreAdapter(
            Func<string, Task<IEnumerable<ITokenMetadata>>> get, 
            Func<string, string, Task> delete)
        {
            if (get == null) throw new ArgumentNullException("get");
            if (delete == null) throw new ArgumentNullException("delete");

            this.get = get;
            this.delete = delete;
        }

        public async Task<IEnumerable<Consent>> LoadAllAsync(string subject)
        {
            var tokens = await get(subject);
            
            var query =
                from token in tokens
                select new Consent
                {
                    ClientId = token.ClientId,
                    Subject = token.SubjectId,
                    Scopes = token.Scopes
                };

            return query.ToArray();
        }

        public async Task RevokeAsync(string subject, string client)
        {
            await delete(subject, client);
        }
    }
}
