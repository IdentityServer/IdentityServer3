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

namespace Thinktecture.IdentityServer.Core.Configuration
{
    public class EndpointOptions
    {
        public EndpointOptions()
        {
            this.EnableAuthorizeEndpoint = true;
            this.EnableTokenEndpoint = true;
            this.EnableUserInfoEndpoint = true;
            this.EnableDiscoveryEndpoint = true;
            this.EnableAccessTokenValidationEndpoint = true;
            this.IdentityTokenValidationEndpoint = true;
            this.EndSessionEndpoint = true;
            this.EnableClientPermissionsEndpoint = true;

            this.EnableCspReportEndpoint = false;
        }

        public bool EnableAuthorizeEndpoint { get; set; }
        public bool EnableTokenEndpoint { get; set; }
        public bool EnableUserInfoEndpoint { get; set; }
        public bool EnableDiscoveryEndpoint { get; set; }
        public bool EnableAccessTokenValidationEndpoint { get; set; }
        public bool IdentityTokenValidationEndpoint { get; set; }
        public bool EndSessionEndpoint { get; set; }
        public bool EnableClientPermissionsEndpoint { get; set; }
        public bool EnableCspReportEndpoint { get; set; }
    }

}
