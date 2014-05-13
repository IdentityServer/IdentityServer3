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
using Thinktecture.IdentityServer.Core.Services;
using Thinktecture.IdentityServer.WsFed.Validation;

namespace Thinktecture.IdentityServer.WsFed.ResponseHandling
{
    public class SignInResponseGenerator
    {
        private ILogger _logger;
        private ICoreSettings _settings;
        
        public SignInResponseGenerator(ILogger logger, ICoreSettings settings)
        {
            _logger = logger;
            _settings = settings;
        }

        public SignInResponseMessage GenerateResponse(SignInValidationResult validationResult)
        {
            // create subject
            var outgoingSubject = CreateSubject(validationResult);

            // create token for user
            var token = CreateSecurityToken(validationResult, outgoingSubject);

            // return response
            var rstr = new RequestSecurityTokenResponse
            {
                AppliesTo = new EndpointReference(validationResult.RelyingParty.Realm),
                Context = validationResult.SignInRequestMessage.Context,
                ReplyTo = validationResult.ReplyUrl,
                RequestedSecurityToken = new RequestedSecurityToken(token)
            };

            var serializer = new WSFederationSerializer(
                new WSTrust13RequestSerializer(),
                new WSTrust13ResponseSerializer());

            var responseMessage = new SignInResponseMessage(
                new Uri(validationResult.ReplyUrl),
                rstr,
                serializer,
                new WSTrustSerializationContext());

            return responseMessage;
        }

        // todo: use user service
        private ClaimsIdentity CreateSubject(SignInValidationResult validationResult)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, "dominick")
            };

            return new ClaimsIdentity(claims, "idsrv");
        }

        private SecurityToken CreateSecurityToken(SignInValidationResult validationResult, ClaimsIdentity outgoingSubject)
        {
            var descriptor = new SecurityTokenDescriptor
            {
                AppliesToAddress = validationResult.RelyingParty.Realm,
                Lifetime = new Lifetime(DateTime.UtcNow, DateTime.UtcNow.AddMinutes(validationResult.RelyingParty.TokenLifeTime)),
                ReplyToAddress = validationResult.ReplyUrl,
                SigningCredentials = new X509SigningCredentials(_settings.GetSigningCertificate()),
                Subject = outgoingSubject,
                TokenIssuerName = _settings.GetIssuerUri(),
                TokenType = validationResult.RelyingParty.TokenType
            };

            return CreateSupportedSecurityTokenHandler().CreateToken(descriptor);
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

    }
}