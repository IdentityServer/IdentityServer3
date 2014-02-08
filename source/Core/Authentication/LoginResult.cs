using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using Thinktecture.IdentityServer.Core.Services;

namespace Thinktecture.IdentityServer.Core.Authentication
{
    public class LoginResult : IHttpActionResult
    {
        SignInMessage _message;
        HttpRequestMessage _request;
        private ICoreSettings _settings;

        public LoginResult(SignInMessage message, HttpRequestMessage request, ICoreSettings settings)
        {
            _message = message;
            _settings = settings;
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
                //var returnUrl = WebUtility.UrlEncode(_request.RequestUri.AbsoluteUri);

                var message = new SignInMessage
                {
                    ReturnUrl = _request.RequestUri.AbsoluteUri
                };

                var protection = _settings.GetInternalProtectionSettings();
                var jwt = message.ToJwt(
                    protection.Issuer,
                    protection.Audience,
                    protection.SigningKey,
                    protection.Ttl);

                // todo: fix relative paths (using OWIN magic)
                var uri = new Uri(_request.RequestUri, "/core/login?message=" + jwt);
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
