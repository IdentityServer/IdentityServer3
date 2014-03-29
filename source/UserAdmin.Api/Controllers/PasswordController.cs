using System;
using System.Threading.Tasks;
using System.Web.Http;
using Thinktecture.IdentityServer.Admin.Core;
using Thinktecture.IdentityServer.UserAdmin.Api.Models;

namespace Thinktecture.IdentityServer.UserAdmin.Api.Controllers
{
    [Route("password")]
    public class PasswordController : ApiController
    {
        IUserManager userManager;
        public PasswordController(IUserManager userManager)
        {
            if (userManager == null) throw new ArgumentNullException("userManager");

            this.userManager = userManager;
        }

        public async Task<IHttpActionResult> PostAsync(SetPassword model)
        {
            if (model == null)
            {
                ModelState.AddModelError("", "Data required");
            }

            if (ModelState.IsValid)
            {
                var result = await this.userManager.SetPasswordAsync(model.Subject, model.Password);
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
