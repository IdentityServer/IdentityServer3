using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Thinktecture.IdentityServer.Core.Configuration
{
    public class AuthenticationOptions
    {
        public IEnumerable<LoginPageLink> LoginPageLinks { get; set; }
    }

    public class LoginPageLink
    {
        public string Text { get; set; }
        public string Href { get; set; }
    }
}
