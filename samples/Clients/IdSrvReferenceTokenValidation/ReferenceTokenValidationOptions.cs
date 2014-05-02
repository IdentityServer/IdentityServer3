using Microsoft.Owin.Security;

namespace IdSrvReferenceTokenValidation
{
    public class ReferenceTokenValidationOptions : AuthenticationOptions
    {
        public ReferenceTokenValidationOptions() : base("IdSrv")
        { }
        
        public string TokenValidationEndpoint { get; set; }
        public string RequiredScope { get; set; }
    }
}