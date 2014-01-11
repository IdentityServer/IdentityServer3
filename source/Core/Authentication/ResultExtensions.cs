using System.Web.Http;

namespace Thinktecture.IdentityServer.Core.Authentication
{
    public static class ResultExtensions
    {
        public static IHttpActionResult Login(this ApiController controller, SignInMessage message)
        {
            return new LoginResult(message, controller.Request);
        }
    }
}
