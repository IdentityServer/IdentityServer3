using System.Linq;
using Microsoft.Owin;
using Owin;
using Thinktecture.IdentityModel;
using System.IdentityModel.Tokens;
using Thinktecture.IdentityModel.Tokens;
using IdSrvReferenceTokenValidation;

[assembly: OwinStartup(typeof(SampleAspNetWebApi.Startup))]

namespace SampleAspNetWebApi
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            JwtSecurityTokenHandler.InboundClaimTypeMap = ClaimMappings.None;

            app.UseJsonWebToken(
                issuer:    "https://idsrv3.com",
                audience:  "https://idsrv3.com/resources",
                signingKey: X509.LocalMachine.TrustedPeople.SubjectDistinguishedName.Find("CN=idsrv3test", false).First());

            app.UseIdentitiyServerReferenceTokens(new ReferenceTokenValidationOptions
                {
                    TokenValidationEndpoint = "http://localhost:3333/core/connect/accessTokenValidation"
                });

            app.UseWebApi(WebApiConfig.Register());
        }
    }
}