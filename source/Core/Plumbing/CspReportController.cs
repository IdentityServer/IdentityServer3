using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using System.Net.Http;
using Thinktecture.IdentityServer.Core.Logging;
using System.Net;

namespace Thinktecture.IdentityServer.Core.Plumbing
{
    public class CspReportController : ApiController
    {
        private readonly static ILog Logger = LogProvider.GetCurrentClassLogger();

        [Route(Constants.RoutePaths.CspReport, Name=Constants.RouteNames.CspReport)]
        public async Task<IHttpActionResult> Post()
        {
            var json = await Request.Content.ReadAsStringAsync();
            Logger.Error(json);
            return ResponseMessage(Request.CreateResponse(HttpStatusCode.NoContent));
        }
    }
}
