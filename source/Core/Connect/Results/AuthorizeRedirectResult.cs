using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using Thinktecture.IdentityServer.Core.Connect.Models;
using Thinktecture.IdentityServer.Core.Extensions;
using Thinktecture.IdentityServer.Core.Logging;

namespace Thinktecture.IdentityServer.Core.Connect.Results
{
    public class AuthorizeRedirectResult : IHttpActionResult
    {
        private readonly static ILog Logger = LogProvider.GetCurrentClassLogger();

        private readonly AuthorizeResponse _response;

        public AuthorizeRedirectResult(AuthorizeResponse response)
        {
            _response = response;
        }

        public Task<HttpResponseMessage> ExecuteAsync(System.Threading.CancellationToken cancellationToken)
        {
            return Task.FromResult(Execute());
        }

        HttpResponseMessage Execute()
        {
            string character = "#";
            var responseMessage = new HttpResponseMessage(HttpStatusCode.Redirect);
            var url = _response.RedirectUri.AbsoluteUri;

            if (_response.Request.ResponseMode == Constants.ResponseModes.Query)
            {
                character = "?";
            }

            var query = _response.ToNameValueCollection();

            url = string.Format("{0}{1}{2}", 
                url,
                character,
                query.ToQueryString());
            
            responseMessage.Headers.Location = new Uri(url);
            Logger.Info("Redirecting to: " + url);

            return responseMessage;
        }
    }
}