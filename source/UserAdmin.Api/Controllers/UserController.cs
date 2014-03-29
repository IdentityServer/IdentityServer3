using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using Thinktecture.IdentityServer.Admin.Core;
using Thinktecture.IdentityServer.UserAdmin.Api.Models;

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

        public async Task<IHttpActionResult> GetAsync(string filter = null, int start = 0, int count = 100)
        {
            var result = await userManager.QueryUsersAsync(filter, start, count);
            if (result.IsSuccess)
            {
                return Ok(result.Result);
            }

            return ResponseMessage(Request.CreateResponse(HttpStatusCode.InternalServerError, result.Errors));
        }

        public async Task<IHttpActionResult> GetAsync(string subject)
        {
            var result = await this.userManager.GetUserAsync(subject);
            if (result.IsSuccess)
            {
                return Ok(result.Result);
            }

            return ResponseMessage(Request.CreateResponse(HttpStatusCode.InternalServerError, result.Errors));
        }

        public async Task<IHttpActionResult> PostAsync(CreateUser model)
        {
            if (model == null)
            {
                ModelState.AddModelError("", "Data required");
            }

            if (ModelState.IsValid)
            {
                var result = await this.userManager.CreateUserAsync(model.Username, model.Password);
                if (result.IsSuccess)
                {
                    return Ok(result.Result);
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error);
                }
            }

            return BadRequest(ModelState.GetErrorMessage());
        }

        [Route("users/delete")]
        [HttpPost]
        public async Task<IHttpActionResult> DeleteUserAsync(DeleteUser model)
        {
            if (model == null)
            {
                ModelState.AddModelError("", "Data required");
            }

            if (ModelState.IsValid)
            {
                var result = await this.userManager.DeleteUserAsync(model.Subject);
                if (result.IsSuccess)
                {
                    return Ok();
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
