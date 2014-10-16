using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Thinktecture.IdentityServer.Core.Models;

namespace Thinktecture.IdentityServer.Core.Connect
{
    public class ValidatedEndSessionRequest : ValidatedRequest
    {
        public Client Client { get; set; }
        public Uri PostLogOutUri { get; set; }
        public string State { get; set; }
    }
}