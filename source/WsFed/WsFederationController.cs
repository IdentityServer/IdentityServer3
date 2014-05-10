using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;

namespace Thinktecture.IdentityServer.WsFed
{
    public class WsFederationController : ApiController
    {
        [Route("wsfed")]
        public IHttpActionResult Get()
        {
            return Ok("okay!");
        }

    }
}
