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
using System.Linq;
using System.Threading.Tasks;
using Thinktecture.IdentityServer.Core.Models;

namespace Thinktecture.IdentityServer.Core.Services.Default
{
    public class DefaultRedirectUriValidator : IRedirectUriValidator
    {
        bool UriCollectionContainsUri(IEnumerable<string> collection, string requestedUri)
        {
            bool result = false;

            Uri uri;
            if (Uri.TryCreate(requestedUri, UriKind.Absolute, out uri))
            {
                var uris = collection.Select(x => new Uri(x));
                result = uris.Contains(uri);
            }

            return result;
        }

        public Task<bool> IsRedirecUriValidAsync(string requestedUri, Client client)
        {
            return Task.FromResult(UriCollectionContainsUri(client.RedirectUris, requestedUri));
        }

        public Task<bool> IsPostLogoutRedirecUriValidAsync(string requestedUri, Client client)
        {
            return Task.FromResult(UriCollectionContainsUri(client.PostLogoutRedirectUris, requestedUri));
        }
    }
}
