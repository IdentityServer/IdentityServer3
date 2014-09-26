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
using System.Collections.Generic;
using Thinktecture.IdentityServer.Core.Configuration;
using Thinktecture.IdentityServer.Core.Services;

namespace Thinktecture.IdentityServer.Core.Views.Embedded
{
    public class EmbeddedAssetsViewServiceRegistration : Registration<IViewService>
    {
        public EmbeddedAssetsViewServiceRegistration(EmbeddedAssetsViewServiceConfiguration config)
        {
            this.TypeFactory = () => new EmbeddedAssetsViewService(config);
        }
    }

    public class EmbeddedAssetsViewServiceConfiguration
    {
        public EmbeddedAssetsViewServiceConfiguration()
        {
            Stylesheets = new HashSet<string>();
            // adding default CSS here so hosting application can choose to remove it
            Stylesheets.Add("~/assets/styles.min.css");
            
            Scripts = new HashSet<string>();
        }
        
        public ICollection<string> Stylesheets { get; set; }
        public ICollection<string> Scripts { get; set; }
    }
}
