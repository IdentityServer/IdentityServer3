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

using IdentityServer3.Core.Configuration;
using IdentityServer3.Core.Events;
using IdentityServer3.Core.Extensions;
using IdentityServer3.Core.Logging;
using IdentityServer3.Core.Services;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web.Http;

namespace IdentityServer3.Core.Endpoints
{
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

        [HttpPost]
        public async Task<IHttpActionResult> Post()
        {
            Logger.Info("CSP Report endpoint requested");

            if (Request.Content.Headers.ContentLength.HasValue && 
                Request.Content.Headers.ContentLength.Value > options.InputLengthRestrictions.CspReport)
            {
                var msg = "Request content exceeds max length";
                Logger.Warn(msg);
                await eventService.RaiseFailureEndpointEventAsync(EventConstants.EndpointNames.CspReport, msg);
                return BadRequest();
            }

            var json = await Request.GetOwinContext().Request.ReadBodyAsStringAsync();
            if (json.Length > options.InputLengthRestrictions.CspReport)
            {
                var msg = "Request content exceeds max length";
                Logger.Warn(msg);
                await eventService.RaiseFailureEndpointEventAsync(EventConstants.EndpointNames.CspReport, msg);
                return BadRequest();
            }

            if (json.IsPresent())
            {
                Logger.InfoFormat("CSP Report data: {0}", json);
                await eventService.RaiseCspReportEventAsync(json, User as ClaimsPrincipal);
            }

            Logger.Info("Rendering 204");
            return ResponseMessage(Request.CreateResponse(HttpStatusCode.NoContent));
        }
    }
}