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

using IdentityServer3.Core;
using IdentityServer3.Core.Configuration;
using IdentityServer3.Core.Configuration.Hosting;
using Microsoft.Owin.Security.DataProtection;

namespace Owin
{
    internal static class ConfigureDataProtectorExtension
    {
        public static IAppBuilder ConfigureDataProtectionProvider(this IAppBuilder app, IdentityServerOptions options)
        {
            if (options.DataProtector == null)
            {
                var provider = app.GetDataProtectionProvider();
                if (provider == null)
                {
                    provider = new DpapiDataProtectionProvider(Constants.PrimaryAuthenticationType);
                }

                options.DataProtector = new HostDataProtector(provider);
            } 
            return app;
        }
    }
}
