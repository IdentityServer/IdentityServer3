using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using Thinktecture.IdentityServer.Core.Connect.Models;

namespace Thinktecture.IdentityServer.Core.Connect.Results
{
    public class AuthorizeCodeResult : IHttpActionResult
    {
        AuthorizeResponse Response { get; set; }

        public AuthorizeCodeResult(AuthorizeResponse response)
        {
            Response = response;
        }

        public Task<HttpResponseMessage> ExecuteAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult<HttpResponseMessage>(Execute());
        }

        private HttpResponseMessage Execute()
        {
            var responseMessage = new HttpResponseMessage(HttpStatusCode.Redirect);

            var url = string.Format("{0}?code={1}", Response.RedirectUri.AbsoluteUri, Response.Code);

            if (Response.State.IsPresent())
            {
                url = string.Format("{0}&state={1}", url, Response.State);
            }

            responseMessage.Headers.Location = new Uri(url);
            return responseMessage;
        }
    }
}
