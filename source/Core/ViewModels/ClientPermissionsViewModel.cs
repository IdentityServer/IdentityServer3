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
using System.Collections.Generic;

namespace IdentityServer3.Core.ViewModels
{
    /// <summary>
    /// Models the data needed to render the client permissions page.
    /// </summary>
    public class ClientPermissionsViewModel : ErrorViewModel
    {
        /// <summary>
        /// The URL to POST to revoke client permissions. <see cref="RevokeClientPermission"/> for the model for the submitted data.
        /// </summary>
        /// <value>
        /// The revoke permission URL.
        /// </value>
        public string RevokePermissionUrl { get; set; }

        /// <summary>
        /// The list of clients and their permissions for the current logged in user.
        /// </summary>
        /// <value>
        /// The clients.
        /// </value>
        public IEnumerable<ClientPermission> Clients { get; set; }

        /// <summary>
        /// The anti forgery values.
        /// </summary>
        /// <value>
        /// The anti forgery.
        /// </value>
        public AntiForgeryTokenViewModel AntiForgery { get; set; }
    }
}
