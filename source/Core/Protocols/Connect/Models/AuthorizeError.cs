using System;

namespace Thinktecture.IdentityServer.Core.Protocols.Connect.Models
{
    public class AuthorizeError
    {
        public ErrorTypes ErrorType { get; set; }
        public string Error { get; set; }
        public string ResponseMode { get; set; }
        public Uri ErrorUri { get; set; }
        public string State { get; set; }
    }
}
