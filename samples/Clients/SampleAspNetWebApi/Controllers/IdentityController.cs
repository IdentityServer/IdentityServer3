using System.Linq;
using System.Security.Claims;
using System.Web.Http;

namespace SampleAspNetWebApi.Controllers
{
    [Authorize]
    public class IdentityController : ApiController
    {
        public dynamic Get()
        {
            var principal = User as ClaimsPrincipal;

            return from c in principal.Identities.First().Claims
                   select new 
                   {
                       c.Type,
                       c.Value
                   };
        }
    }
}