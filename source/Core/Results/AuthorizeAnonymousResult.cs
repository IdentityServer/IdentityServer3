namespace IdentityServer3.Core.Results
{
    using System;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Security.Claims;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Web.Http;

    using IdentityServer3.Core.Configuration.Hosting;
    using IdentityServer3.Core.Extensions;
    using IdentityServer3.Core.Models;

    using Microsoft.Owin;

    public class AuthorizeAnonymousResult : IHttpActionResult
    {
        private readonly SessionCookie sessionCookie;

        private readonly HttpRequestMessage httpRequestMessage;

        private readonly ClaimsPrincipal principal;

        private readonly IOwinContext owinContext;

        public AuthorizeAnonymousResult(HttpRequestMessage httpRequestMessage, AuthorizeResponse response, ClaimsPrincipal principal)
        {
            this.httpRequestMessage = httpRequestMessage;
            this.principal = principal;
            owinContext = httpRequestMessage.GetOwinContext();
            Model = response;
        }

        private void IssueCookies()
        {
            var identity = principal.Identities.First();

            owinContext.Authentication.SignOut(Constants.ExternalAuthenticationType, Constants.PartialSignInAuthenticationType);

            owinContext.Environment.IssueLoginCookie(
                new AuthenticatedLogin
                    {
                        AuthenticationMethod = "anon",
                        PersistentLogin = false,
                        IdentityProvider = "anon",
                        Claims = identity.Claims,
                        Subject = identity.GetSubjectId(),
                        Name = identity.GetSubjectId()
                    });
        }

        public AuthorizeResponse Model { get; private set; }

        public Task<HttpResponseMessage> ExecuteAsync(CancellationToken cancellationToken)
        {
            IssueCookies();

            var responsePayload = new JwtTokenResponse()
                                      {
                                          IdToken = Model.IdentityToken,
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