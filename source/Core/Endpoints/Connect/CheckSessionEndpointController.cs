﻿/*
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

using System.Web.Http;
using Thinktecture.IdentityServer.Core.App_Packages.LibLog._2._0;
using Thinktecture.IdentityServer.Core.Configuration;
using Thinktecture.IdentityServer.Core.Results;

#pragma warning disable 1591

namespace Thinktecture.IdentityServer.Core.Endpoints.Connect
{
    /// <summary>
    /// Check session iframe endpoint
    /// </summary>
    internal class CheckSessionEndpointController : ApiController
    {
        private readonly static ILog Logger = LogProvider.GetCurrentClassLogger();
        private readonly IdentityServerOptions _options;

        public CheckSessionEndpointController(IdentityServerOptions options)
        {
            _options = options;
        }

        /// <summary>
        /// GET
        /// </summary>
        /// <returns>Check session iframe page</returns>
        [Route(Constants.RoutePaths.Oidc.CHECK_SESSION, Name=Constants.RouteNames.Oidc.CHECK_SESSION)]
        public IHttpActionResult Get()
        {
            Logger.Info("Check session iframe request");

            if (!_options.Endpoints.EnableCheckSessionEndpoint)
            {
                Logger.Warn("Endpoint is disabled. Aborting");
                return NotFound();
            }

            return new CheckSessionResult(_options, Request);
        }
   }
}