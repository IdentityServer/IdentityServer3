using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;

namespace Thinktecture.IdentityServer.Core.Authentication
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

                // todo: fix relative paths (using OWIN magic)
                var uri = new Uri(_request.RequestUri, "/core/login?returnUrl=" + returnUrl);
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
