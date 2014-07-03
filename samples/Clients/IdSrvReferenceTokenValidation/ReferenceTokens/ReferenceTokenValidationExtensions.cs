using Microsoft.Owin.Security.OAuth;
using Thinktecture.IdentityServer.v3.AccessTokenValidation;

namespace Owin
{
    public static class ReferenceTokenValidationExtensions
    {
        public static IAppBuilder UseIdentitiyServerReferenceToken(this IAppBuilder app, ReferenceTokenValidationOptions options)
        {
            app.UseOAuthBearerAuthentication(new OAuthBearerAuthenticationOptions
                {
                    AccessTokenProvider = new ReferenceTokenProvider(options.TokenValidationEndpoint, options.AuthenticationType)
                });

            return app;
        }
    }
}