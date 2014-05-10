using Microsoft.IdentityModel.Protocols;
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
            var msg = WsFederationMessage.FromUri(Request.RequestUri);
            if (msg.IsSignInMessage)
            {
                return ProcessSignIn(msg);
            }

            if (msg.IsSignOutMessage)
            {
                return ProcessSignOut(msg);
            }

            return BadRequest();
        }

        private IHttpActionResult ProcessSignIn(WsFederationMessage msg)
        {
            return Ok("signin");
        }

        private IHttpActionResult ProcessSignOut(WsFederationMessage msg)
        {
            return Ok("signout");
        }
    }
}
