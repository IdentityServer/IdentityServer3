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
using Thinktecture.IdentityModel;
using Thinktecture.IdentityServer.Core;
using Thinktecture.IdentityServer.Core.Configuration;
using Thinktecture.IdentityServer.Core.Logging;
using Thinktecture.IdentityServer.Core.Services;
using Thinktecture.IdentityServer.WsFederation.Validation;

namespace Thinktecture.IdentityServer.WsFederation.ResponseHandling
{
    public class SignInResponseGenerator
    {
        private readonly static ILog Logger = LogProvider.GetCurrentClassLogger();
        private readonly CoreSettings _settings;
        private readonly IUserService _users;
        
        public SignInResponseGenerator(CoreSettings settings, IUserService users)
        {
            _settings = settings;
            _users = users;
        }

        public async Task<SignInResponseMessage> GenerateResponseAsync(SignInValidationResult validationResult)
        {
            Logger.Info("Creating WS-Federation signin response");

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
                SigningCredentials = new X509SigningCredentials(_settings.SigningCertificate),
                Subject = outgoingSubject,
                TokenIssuerName = _settings.IssuerUri,
                TokenType = validationResult.RelyingParty.TokenType
            };

            if (validationResult.RelyingParty.EncryptingCertificate != null)
            {
                descriptor.EncryptingCredentials = new X509EncryptingCredentials(validationResult.RelyingParty.EncryptingCertificate);
            }

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