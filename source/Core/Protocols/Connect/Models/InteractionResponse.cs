using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Thinktecture.IdentityServer.Core.Authentication;

namespace Thinktecture.IdentityServer.Core.Protocols.Connect.Models
{
    public class InteractionResponse
    {
        public bool IsError { get; set; }
        public bool IsLogin { get; set; }
        public bool IsConsent { get; set; }

        public AuthorizeError Error { get; set; }
        public SignInMessage SignInMessage { get; set; }
    }
}
