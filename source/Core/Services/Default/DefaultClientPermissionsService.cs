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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Thinktecture.IdentityServer.Core.Extensions;
using Thinktecture.IdentityServer.Core.Models;

namespace Thinktecture.IdentityServer.Core.Services.Default
{
    /// <summary>
    /// Default client permission service
    /// </summary>
    public class DefaultClientPermissionsService : IClientPermissionsService
    {
        readonly IPermissionsStore _permissionsStore;
        readonly IClientStore _clientStore;
        readonly IScopeStore _scopeStore;
        readonly ILocalizationService _localizationService;

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultClientPermissionsService" /> class.
        /// </summary>
        /// <param name="permissionsStore">The permissions store.</param>
        /// <param name="clientStore">The client store.</param>
        /// <param name="scopeStore">The scope store.</param>
        /// <param name="localizationService">The localization service.</param>
        /// <exception cref="System.ArgumentNullException">permissionsStore
        /// or
        /// clientStore
        /// or
        /// scopeStore</exception>
        public DefaultClientPermissionsService(
            IPermissionsStore permissionsStore, 
            IClientStore clientStore, 
            IScopeStore scopeStore,
            ILocalizationService localizationService)
        {
            if (permissionsStore == null) throw new ArgumentNullException("permissionsStore");
            if (clientStore == null) throw new ArgumentNullException("clientStore");
            if (scopeStore == null) throw new ArgumentNullException("scopeStore");
            if (localizationService == null) throw new ArgumentNullException("localizationService");

            _permissionsStore = permissionsStore;
            _clientStore = clientStore;
            _scopeStore = scopeStore;
            _localizationService = localizationService;
        }

        /// <summary>
        /// Gets the client permissions asynchronous.
        /// </summary>
        /// <param name="subject">The subject identifier.</param>
        /// <returns>
        /// A list of client permissions
        /// </returns>
        /// <exception cref="System.ArgumentNullException">subject</exception>
        public virtual async Task<IEnumerable<ClientPermission>> GetClientPermissionsAsync(string subject)
        {
            if (String.IsNullOrWhiteSpace(subject))
            {
                throw new ArgumentNullException("subject");
            }

            var consents = await _permissionsStore.LoadAllAsync(subject);

            var scopesNeeded = consents.Select(x => x.Scopes).SelectMany(x=>x).Distinct();
            var scopes = await _scopeStore.FindScopesAsync(scopesNeeded);
            
            var list = new List<ClientPermission>();
            foreach(var consent in consents)
            {
                var client = await _clientStore.FindClientByIdAsync(consent.ClientId);
                if (client != null)
                {
                    var identityScopes =
                        from s in scopes
                        where s.Type == ScopeType.IDENTITY && consent.Scopes.Contains(s.Name)
                        select new ClientPermissionDescription
                        {
                            DisplayName = s.DisplayName ?? _localizationService.GetScopeDisplayName(s.Name),
                            Description = s.Description ?? _localizationService.GetScopeDescription(s.Name)
                        };

                    var resourceScopes =
                        from s in scopes
                        where s.Type == ScopeType.RESOURCE && consent.Scopes.Contains(s.Name)
                        select new ClientPermissionDescription
                        {
                            DisplayName = s.DisplayName ?? _localizationService.GetScopeDisplayName(s.Name),
                            Description = s.Description ?? _localizationService.GetScopeDescription(s.Name)
                        };

                    list.Add(new ClientPermission
                    {
                        ClientId = client.ClientId,
                        ClientName = client.ClientName,
                        ClientUrl = client.ClientUri,
                        ClientLogoUrl = client.LogoUri,
                        IdentityPermissions = identityScopes,
                        ResourcePermissions = resourceScopes
                    });
                }
            }

            return list;
        }

        /// <summary>
        /// Revokes the client permissions asynchronous.
        /// </summary>
        /// <param name="subject">The subject identifier.</param>
        /// <param name="clientId">The client identifier.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException">
        /// subject
        /// or
        /// clientId
        /// </exception>
        public virtual async Task RevokeClientPermissionsAsync(string subject, string clientId)
        {
            if (String.IsNullOrWhiteSpace(subject))
            {
                throw new ArgumentNullException("subject");
            }

            if (String.IsNullOrWhiteSpace(clientId))
            {
                throw new ArgumentNullException("clientId");
            }

            await _permissionsStore.RevokeAsync(subject, clientId);
        }
    }
}