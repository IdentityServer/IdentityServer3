using System.IdentityModel.Services;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;

namespace Thinktecture.IdentityServer.WsFed.Results
{
    public class WsFederationResult : IHttpActionResult
    {
        SignInResponseMessage _message;

        public WsFederationResult(SignInResponseMessage message)
        {
            _message = message;
        }

        public Task<HttpResponseMessage> ExecuteAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult(Execute());
        }

        HttpResponseMessage Execute()
        {
            var response = new HttpResponseMessage();
            response.Content = new StringContent(_message.WriteFormPost());

            return response;
        }
    }
}