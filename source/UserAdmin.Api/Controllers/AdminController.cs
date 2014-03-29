using System.Web.Http;

namespace Thinktecture.IdentityServer.UserAdmin.Api.Controllers
{
    [Route("admin")]
    public class AdminController : ApiController
    {
        public IHttpActionResult Get()
        {
            return Ok(new
            {
                username = "Admin Username"
            });
        }
    }
}
