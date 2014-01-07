namespace Thinktecture.IdentityServer.Core.Authentication
{
    public class SignInMessage
    {
        public string DisplayMode { get; set; }
        public string UILocales { get; set; }
        public string LoginHint { get; set; }
        public string AuthenticationMethod { get; set; }
    }
}
