using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using Thinktecture.IdentityServer.Admin.Core;

namespace Thinktecture.IdentityServer.UserAdmin.Api.Controllers
{
    [Route("users")]
    public class UserController : ApiController
    {
        IUserManager userManager;
        public UserController(IUserManager userManager)
        {
            if (userManager == null) throw new ArgumentNullException("userManager");

            this.userManager = userManager;
        }

        public async Task<IHttpActionResult> GetAsync()
        {
            var result = await userManager.QueryAsync(null, 0, 100);
            if (result.IsSuccess)
            {
                return Ok(result.Result);
            }

            return ResponseMessage(Request.CreateResponse(HttpStatusCode.InternalServerError, result.Errors));
        }
    }
}
