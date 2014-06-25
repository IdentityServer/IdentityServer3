/*
 * Copyright (c) Dominick Baier, Brock Allen.  All rights reserved.
 * see license
 */
using System;
using System.IdentityModel.Metadata;
using System.IdentityModel.Protocols.WSTrust;
using System.IdentityModel.Tokens;
using Thinktecture.IdentityModel.Constants;
using Thinktecture.IdentityServer.Core.Configuration;

namespace Thinktecture.IdentityServer.WsFederation.ResponseHandling
{
    public class MetadataResponseGenerator
    {
        private CoreSettings _settings;

        public MetadataResponseGenerator(CoreSettings settings)
        {
            _settings = settings;
        }

        public EntityDescriptor Generate(string wsfedEndpoint)
        {
            var tokenServiceDescriptor = GetTokenServiceDescriptor(wsfedEndpoint);

            var id = new EntityId(_settings.IssuerUri);
            var entity = new EntityDescriptor(id);
            entity.SigningCredentials = new X509SigningCredentials(_settings.SigningCertificate);
            entity.RoleDescriptors.Add(tokenServiceDescriptor);

            return entity;
        }

        private SecurityTokenServiceDescriptor GetTokenServiceDescriptor(string wsfedEndpoint)
        {
            var tokenService = new SecurityTokenServiceDescriptor();
            tokenService.ServiceDescription = _settings.SiteName;
            tokenService.Keys.Add(GetSigningKeyDescriptor());

            tokenService.PassiveRequestorEndpoints.Add(new EndpointReference(wsfedEndpoint));
            tokenService.SecurityTokenServiceEndpoints.Add(new EndpointReference(wsfedEndpoint));

            tokenService.TokenTypesOffered.Add(new Uri(TokenTypes.OasisWssSaml11TokenProfile11));
            tokenService.TokenTypesOffered.Add(new Uri(TokenTypes.OasisWssSaml2TokenProfile11));
            tokenService.TokenTypesOffered.Add(new Uri(TokenTypes.JsonWebToken));

            tokenService.ProtocolsSupported.Add(new Uri("http://docs.oasis-open.org/wsfed/federation/200706"));

            return tokenService;
        }

        private KeyDescriptor GetSigningKeyDescriptor()
        {
            var certificate = _settings.SigningCertificate;

            var clause = new X509SecurityToken(certificate).CreateKeyIdentifierClause<X509RawDataKeyIdentifierClause>();
            var key = new KeyDescriptor(new SecurityKeyIdentifier(clause));
            key.Use = KeyType.Signing;

            return key;
        }
    }
}