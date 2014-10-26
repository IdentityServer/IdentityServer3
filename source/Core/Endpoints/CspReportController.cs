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

using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using Thinktecture.IdentityServer.Core.Configuration;
using Thinktecture.IdentityServer.Core.Logging;

namespace Thinktecture.IdentityServer.Core.Endpoints
{
    public class CspReportController : ApiController
    {
        private readonly static ILog Logger = LogProvider.GetCurrentClassLogger();

        readonly IdentityServerOptions options;
        
        public CspReportController(IdentityServerOptions options)
        {
            this.options = options;
        }

        [Route(Constants.RoutePaths.CspReport, Name=Constants.RouteNames.CspReport)]
        public async Task<IHttpActionResult> Post()
        {
            if (!options.CspOptions.ReportEndpoint.IsEnabled)
            {
                return NotFound();
            }

            var json = await Request.Content.ReadAsStringAsync();
            Logger.Error(json);
            return ResponseMessage(Request.CreateResponse(HttpStatusCode.NoContent));
        }
    }
}