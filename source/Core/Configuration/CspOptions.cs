
namespace Thinktecture.IdentityServer.Core.Configuration
{
    public class CspOptions
    {
        public CspOptions()
        {
            Enabled = true;
            ReportEndpoint = EndpointSettings.Disabled;
        }

        public bool Enabled { get; set; }
        public EndpointSettings ReportEndpoint { get; set; }
        public string ScriptSrc { get; set; }
        public string StyleSrc { get; set; }
    }
}
