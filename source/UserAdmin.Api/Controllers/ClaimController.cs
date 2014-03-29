using System;
using System.Threading.Tasks;
using System.Web.Http;
using Thinktecture.IdentityServer.Admin.Core;
using Thinktecture.IdentityServer.UserAdmin.Api.Models;

namespace Thinktecture.IdentityServer.UserAdmin.Api.Controllers
{
    [RoutePrefix("claims")]
    public class ClaimController : ApiController
    {
        IUserManager userManager;
        public ClaimController(IUserManager userManager)
        {
            if (userManager == null) throw new ArgumentNullException("userManager");

            this.userManager = userManager;
        }

        [Route("add")]
        [HttpPost]
        public async Task<IHttpActionResult> AddClaim(Claim model)
        {
            if (model == null)
            {
                ModelState.AddModelError("", "Data required");
            }

            if (ModelState.IsValid)
            {
                var result = await this.userManager.AddClaimAsync(model.Subject, model.Type, model.Value);
                if (result.IsSuccess)
                {
                    return Ok(UserManagerResult.Success);
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error);
                }
            }

            return BadRequest(ModelState.GetErrorMessage());
        }

        [Route("remove")]
        [HttpPost]
        public async Task<IHttpActionResult> RemoveClaim(Claim model)
        {
            if (model == null)
            {
                ModelState.AddModelError("", "Data required");
            }

            if (ModelState.IsValid)
            {
                var result = await this.userManager.DeleteClaimAsync(model.Subject, model.Type, model.Value);
                if (result.IsSuccess)
                {
                    return Ok(UserManagerResult.Success);
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error);
                }
            }

            return BadRequest(ModelState.GetErrorMessage());
        }
    }
}
