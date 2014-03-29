/*
 * Copyright (c) Dominick Baier, Brock Allen.  All rights reserved.
 * see license
 */

using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;

namespace Thinktecture.IdentityServer.Core.Assets
{
    class EmbeddedHtmlResult : IHttpActionResult
    {
        HttpRequestMessage request;
        LayoutModel model;
        public EmbeddedHtmlResult(HttpRequestMessage request, LayoutModel model)
        {
            this.request = request;
            this.model = model;
        }

        public Task<System.Net.Http.HttpResponseMessage> ExecuteAsync(System.Threading.CancellationToken cancellationToken)
        {
            return Task.FromResult(GetResponseMessage(this.request, this.model));
        }

        public static HttpResponseMessage GetResponseMessage(HttpRequestMessage request, LayoutModel model)
        {
            var root = request.GetRequestContext().VirtualPathRoot;
            var html = AssetManager.GetLayoutHtml(model, root);
            return new HttpResponseMessage()
            {
                Content = new StringContent(html, Encoding.UTF8, "text/html")
            };
        }
    }
}
