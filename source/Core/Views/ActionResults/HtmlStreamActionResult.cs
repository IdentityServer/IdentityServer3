/*
 * Copyright (c) Dominick Baier, Brock Allen.  All rights reserved.
 * see license
 */

using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;

namespace Thinktecture.IdentityServer.Core.Views
{
    class HtmlStreamActionResult : IHttpActionResult
    {
        Func<Task<Stream>> renderFunc;
        public HtmlStreamActionResult(Func<Task<Stream>> renderFunc)
        {
            this.renderFunc = renderFunc;
        }

        async Task<Stream> Render()
        {
            return await renderFunc();
        }

        public async Task<HttpResponseMessage> GetResponseMessage()
        {
            var stream = await Render();

            var content = new StreamContent(stream);
            content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("text/html")
            {
                CharSet = Encoding.UTF8.WebName
            };

            return new HttpResponseMessage()
            {
                Content = content
            };
        }

        public async Task<System.Net.Http.HttpResponseMessage> ExecuteAsync(System.Threading.CancellationToken cancellationToken)
        {
            return await GetResponseMessage();
        }
    }
}
