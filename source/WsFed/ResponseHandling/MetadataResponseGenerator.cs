/*
 * Copyright (c) Dominick Baier, Brock Allen.  All rights reserved.
 * see license
 */
using System;
using System.IdentityModel.Metadata;
using System.IdentityModel.Protocols.WSTrust;
using System.IdentityModel.Tokens;
using Thinktecture.IdentityModel.Constants;
using Thinktecture.IdentityServer.Core.Services;

namespace Thinktecture.IdentityServer.WsFed.ResponseHandling
{
    public class MetadataResponseGenerator
    {
        private ILogger _logger;
        private CoreSettings _settings;
        public MetadataResponseGenerator(ILogger logger, CoreSettings settings)
        {
            _logger = logger;
            _settings = settings;
        }

        public EntityDescriptor Generate(string wsfedEndpoint)
        {
            var tokenServiceDescriptor = GetTokenServiceDescriptor(wsfedEndpoint);

            var id = new EntityId(_settings.GetIssuerUri());
            var entity = new EntityDescriptor(id);
            entity.SigningCredentials = new X509SigningCredentials(_settings.GetSigningCertificate());
            entity.RoleDescriptors.Add(tokenServiceDescriptor);

            return entity;
        }

        private SecurityTokenServiceDescriptor GetTokenServiceDescriptor(string wsfedEndpoint)
        {
            var tokenService = new SecurityTokenServiceDescriptor();
            tokenService.ServiceDescription = _settings.GetSiteName();
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
            var certificate = _settings.GetSigningCertificate();

            var clause = new X509SecurityToken(certificate).CreateKeyIdentifierClause<X509RawDataKeyIdentifierClause>();
            var key = new KeyDescriptor(new SecurityKeyIdentifier(clause));
            key.Use = KeyType.Signing;

            return key;
        }

        //private XmlDictionaryReader CreateMetadataReader(Uri mexAddress)
        //{
        //    var metadataSet = new MetadataSet();
        //    var metadataReference = new MetadataReference(new EndpointAddress(mexAddress), AddressingVersion.WSAddressing10);
        //    var metadataSection = new MetadataSection(MetadataSection.MetadataExchangeDialect, null, metadataReference);
        //    metadataSet.MetadataSections.Add(metadataSection);

        //    var sb = new StringBuilder();
        //    var w = new StringWriter(sb, CultureInfo.InvariantCulture);
        //    var writer = XmlWriter.Create(w);

        //    metadataSet.WriteTo(writer);
        //    writer.Flush();
        //    w.Flush();

        //    var input = new StringReader(sb.ToString());
        //    var reader = new XmlTextReader(input);
        //    return XmlDictionaryReader.CreateDictionaryReader(reader);
        //}
    }
}
