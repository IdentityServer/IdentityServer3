using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using Thinktecture.IdentityServer.Core.Protocols.Connect.Models;

namespace Thinktecture.IdentityServer.Core.Protocols.Connect.Results
{
    public class OidcAuthorizeImplicitFragmentResult : IHttpActionResult
    {
        AuthorizeResponse Response { get; set; }

        public OidcAuthorizeImplicitFragmentResult(AuthorizeResponse response)
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

            var url = string.Format("{0}#id_token={1}",
                Response.RedirectUri.AbsoluteUri,
                Response.IdentityToken);

            if (Response.AccessToken.IsPresent())
            {
                url = string.Format("{0}&access_token={1}&token_type=bearer&expires_in={2}", 
                    url, 
                    Response.AccessToken,
                    Response.AccessTokenLifetime);
            }
            if (Response.State.IsPresent())
            {
                url = string.Format("{0}&state={1}", url, Response.State);
            }

            //_logger.InformationFormat("Redirecting back to: {0}", url);

            responseMessage.Headers.Location = new Uri(url);
            return responseMessage;
        }
    }
}
