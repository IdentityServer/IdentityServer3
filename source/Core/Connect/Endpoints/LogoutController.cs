using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using Thinktecture.IdentityServer.Core.Assets;
using Thinktecture.IdentityServer.Core.Services;

namespace Thinktecture.IdentityServer.Core.Connect
{
    public class LogoutController : ApiController
    {
        ILogger logger;
        ICoreSettings settings;
        public LogoutController(ILogger logger, ICoreSettings settings)
        {
            this.logger = logger;
            this.settings = settings;
        }

        [Route("connect/logout")]
        [HttpGet]
        public IHttpActionResult Logout()
        {
            logger.Start("[LogoutController.Logout] called");

            return new EmbeddedHtmlResult(
                Request,
                new LayoutModel
                {
                    Server = settings.GetSiteName(),
                    Page = "logoutprompt",
                    PageModel = new
                    {
                        url = Url.Route("logout", null)
                    }
                });
        }
    }
}
