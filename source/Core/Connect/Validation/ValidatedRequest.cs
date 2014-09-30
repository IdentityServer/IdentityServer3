using System.Collections.Generic;
using System.Collections.Specialized;
using System.Security.Claims;
using Thinktecture.IdentityServer.Core.Configuration;

namespace Thinktecture.IdentityServer.Core.Connect
{
    public class ValidatedRequest
    {
        public NameValueCollection Raw { get; set; }
        public ClaimsPrincipal Subject { get; set; }
        public IDictionary<string, object> Environment { get; set; }
        public IdentityServerOptions Options { get; set; }
    }
}