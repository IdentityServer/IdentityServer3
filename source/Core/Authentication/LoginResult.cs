using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using Thinktecture.IdentityServer.Core.Services;
using Thinktecture.IdentityServer.Core.Extensions;

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

                var baseUrl = _request.GetBaseUrl(_settings.GetPublicHost());
                var uri = new Uri(baseUrl + "login?message=" + jwt);
                
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
