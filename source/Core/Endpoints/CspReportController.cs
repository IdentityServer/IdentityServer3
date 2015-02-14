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

using System.ComponentModel;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web.Http;
using Thinktecture.IdentityServer.Core.Configuration;
using Thinktecture.IdentityServer.Core.Events;
using Thinktecture.IdentityServer.Core.Extensions;
using Thinktecture.IdentityServer.Core.Logging;
using Thinktecture.IdentityServer.Core.Services;

#pragma warning disable 1591

namespace Thinktecture.IdentityServer.Core.Endpoints
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    [HostAuthentication(Constants.PrimaryAuthenticationType)]
    internal class CspReportController : ApiController
    {
        private readonly static ILog Logger = LogProvider.GetCurrentClassLogger();

        private readonly IdentityServerOptions options;
        private readonly IEventService eventService;

        public CspReportController(IdentityServerOptions options, IEventService eventService)
        {
            this.options = options;
            this.eventService = eventService;
        }

        [Route(Constants.RoutePaths.CspReport, Name=Constants.RouteNames.CspReport)]
        public async Task<IHttpActionResult> Post()
        {
            Logger.Info("CSP Report endpoint requested");

            if (!options.Endpoints.EnableCspReportEndpoint)
            {
                Logger.Error("endpoint disabled, returning 404");
                eventService.RaiseFailureEndpointEvent(EventConstants.EndpointNames.CspReport, "endpoint disabled");
                return NotFound();
            }

            if (Request.Content.Headers.ContentLength.HasValue && 
                Request.Content.Headers.ContentLength.Value > Constants.MaxCspReportLength)
            {
                var msg = "Request content exceeds max length";
                Logger.Warn(msg);
                eventService.RaiseFailureEndpointEvent(EventConstants.EndpointNames.CspReport, msg);
                return BadRequest();
            }

            var json = await Request.Content.ReadAsStringAsync();
            if (json.Length > Constants.MaxCspReportLength)
            {
                var msg = "Request content exceeds max length";
                Logger.Warn(msg);
                eventService.RaiseFailureEndpointEvent(EventConstants.EndpointNames.CspReport, msg);
                return BadRequest();
            }

            if (json.IsPresent())
            {
                Logger.InfoFormat("CSP Report data: {0}", json);
                eventService.RaiseCspReportEvent(json, User as ClaimsPrincipal);
            }

            Logger.Info("Rendering 204");
            return ResponseMessage(Request.CreateResponse(HttpStatusCode.NoContent));
        }
    }
}