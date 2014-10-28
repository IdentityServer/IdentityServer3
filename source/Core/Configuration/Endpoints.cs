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
using System.Text;
using System.Threading.Tasks;

namespace Thinktecture.IdentityServer.Core.Configuration
{
    public class Endpoints
    {
        public Endpoints()
        {
            this.AuthorizeEndpoint = EndpointSettings.Enabled;
            this.TokenEndpoint = EndpointSettings.Enabled;
            this.UserInfoEndpoint = EndpointSettings.Enabled;
            this.DiscoveryEndpoint = EndpointSettings.Enabled;
            this.AccessTokenValidationEndpoint = EndpointSettings.Enabled;
            this.IdentityTokenValidationEndpoint = EndpointSettings.Enabled;
            this.EndSessionEndpoint = EndpointSettings.Enabled;
            this.ClientPermissionsEndpoint = EndpointSettings.Enabled;
        }

        public EndpointSettings AuthorizeEndpoint { get; set; }
        public EndpointSettings TokenEndpoint { get; set; }
        public EndpointSettings UserInfoEndpoint { get; set; }
        public EndpointSettings DiscoveryEndpoint { get; set; }
        public EndpointSettings AccessTokenValidationEndpoint { get; set; }
        public EndpointSettings IdentityTokenValidationEndpoint { get; set; }
        public EndpointSettings EndSessionEndpoint { get; set; }
        public EndpointSettings ClientPermissionsEndpoint { get; set; }
    }

}
