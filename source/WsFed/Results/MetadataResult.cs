/*
 * Copyright (c) Dominick Baier, Brock Allen.  All rights reserved.
 * see license
 */
using System.IdentityModel.Metadata;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using System.Xml;
using Thinktecture.IdentityServer.Core.Logging;

namespace Thinktecture.IdentityServer.WsFederation.Results
{
    public class MetadataResult : IHttpActionResult
    {
        private readonly static ILog Logger = LogProvider.GetCurrentClassLogger();
        private readonly EntityDescriptor _entity;

        public MetadataResult(EntityDescriptor entity)
        {
            _entity = entity;
        }

        public Task<HttpResponseMessage> ExecuteAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult(Execute());
        }

        private HttpResponseMessage Execute()
        {
            var ser = new MetadataSerializer();
            var sb = new StringBuilder(512);

            ser.WriteMetadata(XmlWriter.Create(new StringWriter(sb), new XmlWriterSettings { OmitXmlDeclaration = true }), _entity);

            var content = new StringContent(sb.ToString(), Encoding.UTF8, "application/xml");

            Logger.Debug("Returning WS-Federation metadata response");
            return new HttpResponseMessage { Content = content };
        }
    }
}