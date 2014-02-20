using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;

namespace Thinktecture.IdentityServer.Core.Assets
{
    class EmbeddedHtmlResult : IHttpActionResult
    {
        LayoutModel model;
        public EmbeddedHtmlResult(LayoutModel model)
        {
            this.model = model;
        }

        public Task<System.Net.Http.HttpResponseMessage> ExecuteAsync(System.Threading.CancellationToken cancellationToken)
        {
            var html = AssetManager.GetLayoutHtml(model);
            return Task.FromResult(new HttpResponseMessage()
            {
                Content = new StringContent(html, Encoding.UTF8, "text/html")
            });
        }
    }
}
