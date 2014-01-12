namespace Thinktecture.IdentityServer.Core.Authentication
{
    public class SignInMessage
    {
        // page, popup (todo), touch, wap
        public string DisplayMode { get; set; }
        public string UILocales { get; set; }
        public string LoginHint { get; set; }
        // password, cert, 2fa
        public string AuthenticationMethod { get; set; }

        public string ReturnUrl { get; set; }
        public string LoginPromptCorrelationId { get; set; }
    }
}
