using IdSrvReferenceTokenValidation;
using Microsoft.Owin.Security.OAuth;

namespace Owin
{
    public static class ReferenceTokenValidationExtensions
    {
        public static IAppBuilder UseIdentitiyServerReferenceTokens(this IAppBuilder app, ReferenceTokenValidationOptions options)
        {
            app.UseOAuthBearerAuthentication(new OAuthBearerAuthenticationOptions
                {
                    AccessTokenProvider = new IdSrvReferenceTokenProvider(options.TokenValidationEndpoint)
                });

            return app;
        }
    }
}