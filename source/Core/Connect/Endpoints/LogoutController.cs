using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using Thinktecture.IdentityServer.Core.Assets;

namespace Thinktecture.IdentityServer.Core.Connect
{
    public class LogoutController : ApiController
    {
        [Route("connect/logout")]
        [HttpGet]
        public IHttpActionResult Logout()
        {
            return new EmbeddedHtmlResult(
                Request,
                new LayoutModel
                {
                    Title = "Logout",
                    Page = "logoutprompt",
                    PageModel = new
                    {
                        url = Url.Route("logout", null)
                    }
                });
        }
    }
}
