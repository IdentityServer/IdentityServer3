/*
 * Copyright 2014, 2015 Dominick Baier, Brock Allen
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *   http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;

namespace IdentityServer3.Core.Results
{
    internal class HtmlStreamActionResult : IHttpActionResult
    {
        readonly Func<Task<Stream>> renderFunc;

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

            return new HttpResponseMessage
            {
                Content = content
            };
        }

        public async Task<HttpResponseMessage> ExecuteAsync(System.Threading.CancellationToken cancellationToken)
        {
            return await GetResponseMessage();
        }
    }
}
