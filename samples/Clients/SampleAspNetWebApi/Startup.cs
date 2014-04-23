using System.Linq;
using Microsoft.Owin;
using Owin;
using Thinktecture.IdentityModel;
using System.IdentityModel.Tokens;
using Thinktecture.IdentityModel.Tokens;

[assembly: OwinStartup(typeof(SampleAspNetWebApi.Startup))]

namespace SampleAspNetWebApi
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            JwtSecurityTokenHandler.InboundClaimTypeMap = ClaimMappings.None;

            app.UseJsonWebToken(
                issuer: "https://idsrv3.com",
                audience: "https://idsrv3.com/resources",
                signingKey: X509.LocalMachine.TrustedPeople.SubjectDistinguishedName.Find("CN=idsrv3test", false).First());

            app.UseWebApi(WebApiConfig.Register());
        }
    }
}