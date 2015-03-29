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
using IdentityServer3.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IdentityServer3.Core.Extensions
{
    internal static class IClientStoreExtensions
    {
        internal static async Task<IEnumerable<string>> GetIdentityProviderRestrictionsAsync(this IClientStore store, string clientId)
        {
            if (store == null) throw new ArgumentNullException("store");

            if (clientId.IsPresent())
            {
                var client = await store.FindClientByIdAsync(clientId);
                if (client != null &&
                    client.IdentityProviderRestrictions != null &&
                    client.IdentityProviderRestrictions.Any())
                {
                    return client.IdentityProviderRestrictions;
                }
            }

            return Enumerable.Empty<string>();
        }

        internal static async Task<bool> IsValidIdentityProviderAsync(this IClientStore store, string clientId, string provider)
        {
            var restrictions = await store.GetIdentityProviderRestrictionsAsync(clientId);
            
            if (restrictions.Any())
            {
                return restrictions.Contains(provider);
            }

            return true;
        }

        internal static async Task<string> GetClientName(this IClientStore store, SignOutMessage signOutMessage)
        {
            if (store == null) throw new ArgumentNullException("store");

            if (signOutMessage != null && signOutMessage.ClientId.IsPresent())
            {
                var client = await store.FindClientByIdAsync(signOutMessage.ClientId);
                if (client != null)
                {
                    return client.ClientName;
                }
            }

            return null;
        }
    }
}
