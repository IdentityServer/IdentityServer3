using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using System.Net.Http;
using Thinktecture.IdentityServer.Core.Logging;
using System.Net;
using Thinktecture.IdentityServer.Core.Configuration;

namespace Thinktecture.IdentityServer.Core.Hosting
{
    public class CspReportController : ApiController
    {
        private readonly static ILog Logger = LogProvider.GetCurrentClassLogger();

        IdentityServerOptions options;
        public CspReportController(IdentityServerOptions options)
        {
            this.options = options;
        }

        [Route(Constants.RoutePaths.CspReport, Name=Constants.RouteNames.CspReport)]
        public async Task<IHttpActionResult> Post()
        {
            if (!options.CspReportEndpoint.IsEnabled)
            {
                return NotFound();
            }

            var json = await Request.Content.ReadAsStringAsync();
            Logger.Error(json);
            return ResponseMessage(Request.CreateResponse(HttpStatusCode.NoContent));
        }
    }
}
