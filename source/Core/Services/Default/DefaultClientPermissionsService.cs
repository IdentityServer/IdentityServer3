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
    public class DefaultClientPermissionsService : IClientPermissionsService
    {
        readonly IPermissionsStore permissionsStore;
        readonly IClientStore clientStore;
        readonly IScopeStore scopeStore;

        public DefaultClientPermissionsService(
            IPermissionsStore permissionsStore, 
            IClientStore clientStore, 
            IScopeStore scopeStore)
        {
            if (permissionsStore == null) throw new ArgumentNullException("permissionsStore");
            if (clientStore == null) throw new ArgumentNullException("clientStore");
            if (scopeStore == null) throw new ArgumentNullException("scopeStore");

            this.permissionsStore = permissionsStore;
            this.clientStore = clientStore;
            this.scopeStore = scopeStore;
        }

        public async Task<IEnumerable<ClientPermission>> GetClientPermissionsAsync(string subject)
        {
            if (String.IsNullOrWhiteSpace(subject))
            {
                throw new ArgumentNullException("subject");
            }

            var consents = await this.permissionsStore.LoadAllAsync(subject);
            var list = new List<ClientPermission>();
            foreach(var consent in consents)
            {
                var client = await clientStore.FindClientByIdAsync(consent.ClientId);
                if (client != null)
                {
                    var scopes = await scopeStore.GetScopesAsync();
                    var identityScopes = scopes.Where(x=>x.Type == ScopeType.Identity && consent.Scopes.Contains(x.Name)).Select(x=>new PermissionDescription{DisplayName = x.DisplayName, Description = x.Description});
                    var resourceScopes = scopes.Where(x=>x.Type == ScopeType.Resource && consent.Scopes.Contains(x.Name)).Select(x=>new PermissionDescription{DisplayName = x.DisplayName, Description = x.Description});

                    list.Add(new ClientPermission
                    {
                        ClientId = client.ClientId,
                        ClientName = client.ClientName,
                        ClientUrl = client.ClientUri,
                        ClientLogoUrl = client.LogoUri.AbsoluteUri,
                        IdentityPermissions = identityScopes,
                        ResourcePermissions = resourceScopes
                    });
                }
            }

            return list;
        }

        public async Task RevokeClientPermissionsAsync(string subject, string clientId)
        {
            if (String.IsNullOrWhiteSpace(subject))
            {
                throw new ArgumentNullException("subject");
            }

            if (String.IsNullOrWhiteSpace(clientId))
            {
                throw new ArgumentNullException("clientId");
            }

            await this.permissionsStore.RevokeAsync(subject, clientId);
        }
    }
}
