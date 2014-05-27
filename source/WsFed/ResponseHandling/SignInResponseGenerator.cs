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
using System.Threading.Tasks;
using Thinktecture.IdentityServer.Core.Services;
using Thinktecture.IdentityServer.WsFed.Validation;
using Thinktecture.IdentityServer.Core;
using Thinktecture.IdentityModel;

namespace Thinktecture.IdentityServer.WsFed.ResponseHandling
{
    public class SignInResponseGenerator
    {
        private ILogger _logger;
        private CoreSettings _settings;
        private IUserService _users;
        
        public SignInResponseGenerator(ILogger logger, CoreSettings settings, IUserService users)
        {
            _logger = logger;
            _settings = settings;
            _users = users;
        }

        public async Task<SignInResponseMessage> GenerateResponseAsync(SignInValidationResult validationResult)
        {
            // create subject
            var outgoingSubject = await CreateSubjectAsync(validationResult);

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

        private async Task<ClaimsIdentity> CreateSubjectAsync(SignInValidationResult validationResult)
        {
            var claims = await _users.GetProfileDataAsync(
                validationResult.Subject.GetSubjectId(), 
                validationResult.RelyingParty.ClaimMappings.Keys);

            var mappedClaims = new List<Claim>();

            foreach (var claim in claims)
            {
                string mappedType;
                if (validationResult.RelyingParty.ClaimMappings.TryGetValue(claim.Type, out mappedType))
                {
                    mappedClaims.Add(new Claim(mappedType, claim.Value));
                }
            }

            // todo: do complete mapping
            if (validationResult.Subject.GetAuthenticationMethod() == Constants.AuthenticationMethods.Password)
            {
                mappedClaims.Add(new Claim(ClaimTypes.AuthenticationMethod, AuthenticationMethods.Password));
                mappedClaims.Add(AuthenticationInstantClaim.Now);
            }
            
            return new ClaimsIdentity(mappedClaims, "idsrv");
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