/*
 * Copyright (c) Dominick Baier, Brock Allen.  All rights reserved.
 * see license
 */
using System.IdentityModel.Metadata;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using System.Xml;

namespace Thinktecture.IdentityServer.WsFed.Results
{
    public class MetadataResult : IHttpActionResult
    {
        private EntityDescriptor _entity;

        public MetadataResult(EntityDescriptor entity)
        {
            _entity = entity;
        }

        public Task<System.Net.Http.HttpResponseMessage> ExecuteAsync(System.Threading.CancellationToken cancellationToken)
        {
            return Task.FromResult(Execute());
        }

        HttpResponseMessage Execute()
        {
            var ser = new MetadataSerializer();
            var sb = new StringBuilder(512);

            ser.WriteMetadata(XmlWriter.Create(new StringWriter(sb), new XmlWriterSettings { OmitXmlDeclaration = true }), _entity);

            var content = new StringContent(sb.ToString(), Encoding.UTF8, "application/xml");
            return new HttpResponseMessage { Content = content };
        }
    }
}