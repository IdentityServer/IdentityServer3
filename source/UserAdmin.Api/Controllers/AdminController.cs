using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
