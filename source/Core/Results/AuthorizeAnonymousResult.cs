namespace IdentityServer3.Core.Results
{
    using System;
    using System.Net;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Web.Http;

    using IdentityServer3.Core.Models;

    public class AuthorizeAnonymousResult : IHttpActionResult
    {
        private readonly HttpRequestMessage httpRequestMessage;

        public AuthorizeAnonymousResult(HttpRequestMessage httpRequestMessage, AuthorizeResponse response)
        {
            this.httpRequestMessage = httpRequestMessage;
            Model = response;
        }

        public AuthorizeResponse Model { get; private set; }

        public Task<HttpResponseMessage> ExecuteAsync(CancellationToken cancellationToken)
        {
            var responsePayload = new JwtTokenResponse()
                                      {
                                          AccessToken = Model.AccessToken,
                                          ExpiresIn = Model.AccessTokenLifetime,
                                          Expires = DateTime.UtcNow.AddSeconds(Model.AccessTokenLifetime),
                                          Issued = DateTime.UtcNow,
                                          TokenType = "bearer"
                                      };

            var response = httpRequestMessage.CreateResponse(HttpStatusCode.OK, responsePayload);
            return Task.FromResult(response);
        }
    }
}