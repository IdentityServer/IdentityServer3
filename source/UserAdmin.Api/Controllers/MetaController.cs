using System;
using System.Threading.Tasks;
using System.Web.Http;
using Thinktecture.IdentityServer.Admin.Core;

namespace Thinktecture.IdentityServer.UserAdmin.Api.Controllers
{
    [Route("meta")]
    public class MetaController : ApiController
    {
        IUserManager userManager;
        public MetaController(IUserManager userManager)
        {
            if (userManager == null) throw new ArgumentNullException("userManager");

            this.userManager = userManager;
        }

        public async Task<IHttpActionResult> GetAsync()
        {
            return Ok(await userManager.GetMetadataAsync());
        }
    }
}
