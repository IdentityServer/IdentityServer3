using Microsoft.Owin.Security.Jwt;
using Thinktecture.IdentityServer.v3.AccessTokenValidation;

namespace Owin
{
    public static class SelfContainedTokenValidationExtensions
    {
        public static IAppBuilder UseIdentitiyServerSelfContainedToken(this IAppBuilder app, SelfContainedTokenValidationOptions options)
        {
            var audience = options.IssuerName;

            if (audience.EndsWith("/"))
            {
                audience = options.IssuerName.Substring(0, options.IssuerName.Length - 1);
            }

            audience += "/resources";

            app.UseJwtBearerAuthentication(new Microsoft.Owin.Security.Jwt.JwtBearerAuthenticationOptions
                {
                    AuthenticationType = options.AuthenticationType,

                    AllowedAudiences = new[] { audience },
                    IssuerSecurityTokenProviders = new[] 
                        {
                            new X509CertificateSecurityTokenProvider(
                                options.IssuerName,
                                options.SigningCertificate)
                        }
                });

            return app;
        }
    }
}