using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using Thinktecture.IdentityServer.Core.Connect.Models;

namespace Thinktecture.IdentityServer.Core.Connect.Results
{
    public class AuthorizeImplicitFragmentResult : IHttpActionResult
    {
        AuthorizeResponse Response { get; set; }

        public AuthorizeImplicitFragmentResult(AuthorizeResponse response)
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
            var url = Response.RedirectUri.AbsoluteUri;

            //HttpValueCollection c = new HttpValueCollection();


            if (Response.IdentityToken.IsPresent() && Response.AccessToken.IsPresent())
            {
                url = string.Format("{0}#id_token={1}&access_token={2}&token_type=bearer&expires_in={3}",
                    url,
                    Response.IdentityToken,
                    Response.AccessToken,
                    Response.AccessTokenLifetime);
            }
            else if (Response.IdentityToken.IsPresent())
            {
                url = string.Format("{0}#id_token={1}",
                    url,
                    Response.IdentityToken);

                responseMessage.Headers.Location = new Uri(url);
                return responseMessage;
            }
            else if (Response.AccessToken.IsPresent())
            {
                url = string.Format("{0}#access_token={1}&token_type=bearer&expires_in={2}", 
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
