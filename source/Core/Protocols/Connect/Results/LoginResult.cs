using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using Thinktecture.IdentityServer.Core.Authentication;

namespace Thinktecture.IdentityServer.Core.Protocols.Connect.Results
{
    public class LoginResult : IHttpActionResult
    {
        SignInMessage _message;
        HttpRequestMessage _request;

        public LoginResult(SignInMessage message, HttpRequestMessage request)
        {
            _message = message;
            _request = request;
        }

        public Task<HttpResponseMessage> ExecuteAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult<HttpResponseMessage>(Execute());
        }

        private HttpResponseMessage Execute()
        {
            HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.Redirect);

            try
            {
                var returnUrl = WebUtility.UrlEncode(_request.RequestUri.AbsoluteUri);

                var uri = new Uri(_request.RequestUri, "/login?returnUrl=" + returnUrl);
                response.Headers.Location = uri;
            }
            catch
            {
                response.Dispose();
                throw;
            }

            return response;
        }
    }
}
