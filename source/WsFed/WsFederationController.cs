/*
 * Copyright (c) Dominick Baier, Brock Allen.  All rights reserved.
 * see license
 */
using System;
using System.Collections.Generic;
using System.IdentityModel.Protocols.WSTrust;
using System.IdentityModel.Services;
using System.IdentityModel.Tokens;
using System.Security.Claims;
using System.Web.Http;
using Thinktecture.IdentityServer.Core.Authentication;
using Thinktecture.IdentityServer.Core.Services;
using Thinktecture.IdentityServer.WsFed.Models;
using Thinktecture.IdentityServer.WsFed.Results;
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
            _relyingParties = new TestRelyingPartyService();
        }

        [Route("wsfed")]
        public IHttpActionResult Get()
        {
            WSFederationMessage message;
            if (WSFederationMessage.TryCreateFromUri(Request.RequestUri, out message))
            {
                var signin = message as SignInRequestMessage;
                if (signin != null)
                {
                    return ProcessSignIn(signin);
                }

                var signout = message as SignOutRequestMessage;
                if (signout != null)
                {
                    return ProcessSignOut(signout);
                }
            }

            return BadRequest("Invalid WS-Federation request");
        }

        private IHttpActionResult ProcessSignIn(SignInRequestMessage msg)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return this.RedirectToLogin(_settings);
            }

            // check rp
            var rp = _relyingParties.GetByRealm(msg.Realm);
            
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
                Context = msg.Context,
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

            return new WsFederationResult(responseMessage);
        }

        private ClaimsIdentity CreateSubject(RelyingParty rp)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, "dominick")
            };

            return new ClaimsIdentity(claims, "idsrv");
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

            return CreateSupportedSecurityTokenHandler().CreateToken(descriptor);
        }

        private IHttpActionResult ProcessSignOut(SignOutRequestMessage msg)
        {
            return Ok("signout");
        }

        private SecurityTokenHandlerCollection CreateSupportedSecurityTokenHandler()
        {
            return new SecurityTokenHandlerCollection(new SecurityTokenHandler[]
            {
                new SamlSecurityTokenHandler(),
                new EncryptedSecurityTokenHandler(),
                new Saml2SecurityTokenHandler(),
                new JwtSecurityTokenHandler()
            });
        }


        IHttpActionResult RedirectToLogin(ICoreSettings settings)
        {
            var message = new SignInMessage();
            message.ReturnUrl = Request.RequestUri.AbsoluteUri;

            return new LoginResult(message, this.Request, settings);
        }
    }
}