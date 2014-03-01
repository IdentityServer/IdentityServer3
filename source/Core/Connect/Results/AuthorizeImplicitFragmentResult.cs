using System;
using System.Collections.Specialized;
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
            var query = new NameValueCollection();

            if (Response.IdentityToken.IsPresent())
            {
                query.Add("id_token", Response.IdentityToken);
            }
            
            if (Response.AccessToken.IsPresent())
            {
                query.Add("access_token", Response.AccessToken);
                query.Add("token_type", "Bearer");
                query.Add("expires_in", Response.AccessTokenLifetime.ToString());
            }

            if (Response.State.IsPresent())
            {
                query.Add("state", Response.State);
            }

            url = string.Format("{0}#{1}", url, query.ToQueryString());
            responseMessage.Headers.Location = new Uri(url);
            return responseMessage;
        }
    }
}