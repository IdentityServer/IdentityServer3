using Microsoft.IdentityModel.Protocols;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IdentityModel.Protocols.WSTrust;
using System.IdentityModel.Services;
using System.IdentityModel.Tokens;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using Thinktecture.IdentityServer.Core.Authentication;
using Thinktecture.IdentityServer.Core.Services;
using Thinktecture.IdentityServer.WsFed.Models;
using Thinktecture.IdentityServer.WsFed.Services;

namespace Thinktecture.IdentityServer.WsFed
{
    [HostAuthentication("idsrv")]
    public class WsFederationController : ApiController
    {
        private readonly ICoreSettings _settings;
        private readonly IRelyingPartyService _relyingParties;

        public WsFederationController(ICoreSettings settings)
        {
            _settings = settings;
        }

        [Route("wsfed")]
        public IHttpActionResult Get()
        {
            if (!User.Identity.IsAuthenticated)
            {
                return this.RedirectToLogin(_settings);
            }

            var msg = WsFederationMessage.FromUri(Request.RequestUri);
            if (msg.IsSignInMessage)
            {
                return ProcessSignIn(msg);
            }

            if (msg.IsSignOutMessage)
            {
                return ProcessSignOut(msg);
            }

            return BadRequest();
        }

        private IHttpActionResult ProcessSignIn(WsFederationMessage msg)
        {
            // check rp
            var rp = _relyingParties.GetByRealm(msg.Wtrealm);
            
            if (rp == null)
            {
                return BadRequest();
            }

            // create subject
            var subject = CreateSubject(rp);

            // create token for user
            var token = CreateSecurityToken(rp, subject);

            // return response
            var rstr = new RequestSecurityTokenResponse
            {
                AppliesTo = new EndpointReference(rp.Realm),
                Context = msg.Wctx,
                ReplyTo = rp.ReplyUrl.AbsoluteUri,
                RequestedSecurityToken = new RequestedSecurityToken(token)
            };

            var serializer = new WSFederationSerializer(
                new WSTrust13RequestSerializer(), 
                new WSTrust13ResponseSerializer());

            var responseMessage = new SignInResponseMessage(
                rp.ReplyUrl,
                rstr,
                serializer,
                new WSTrustSerializationContext());



            return Ok("signin");
        }

        private ClaimsIdentity CreateSubject(RelyingParty rp)
        {
            return null;
        }

        private SecurityToken CreateSecurityToken(RelyingParty rp, ClaimsIdentity subject)
        {
            var descriptor = new SecurityTokenDescriptor
            {
                AppliesToAddress = rp.Realm,
                Lifetime = new Lifetime(DateTime.UtcNow, DateTime.UtcNow.AddMinutes(rp.TokenLifeTime)),
                ReplyToAddress = rp.ReplyUrl.AbsoluteUri,
                SigningCredentials = new X509SigningCredentials(_settings.GetSigningCertificate()),
                Subject = subject,
                TokenIssuerName = _settings.GetIssuerUri(),
                TokenType = rp.TokenType
            };

            var handler = SecurityTokenHandlerCollection.CreateDefaultSecurityTokenHandlerCollection();
            return handler.CreateToken(descriptor);            
        }



        private IHttpActionResult ProcessSignOut(WsFederationMessage msg)
        {
            return Ok("signout");
        }

        IHttpActionResult RedirectToLogin(ICoreSettings settings)
        {
            var message = new SignInMessage();
            message.ReturnUrl = Request.RequestUri.AbsoluteUri;

            return new LoginResult(message, this.Request, settings);
        }
    }
}