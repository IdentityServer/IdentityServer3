using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using Thinktecture.IdentityServer.Core.Connect.Models;
using Thinktecture.IdentityServer.Core.Extensions;
using Thinktecture.IdentityServer.Core.Logging;
using Thinktecture.IdentityServer.Core.Views.Embedded.Assets;

namespace Thinktecture.IdentityServer.Core.Connect.Results
{
    public class AuthorizeFormPostResult : IHttpActionResult
    {
        private readonly static ILog Logger = LogProvider.GetCurrentClassLogger();

        private AuthorizeResponse _response;
        private HttpRequestMessage _request;

        public AuthorizeFormPostResult(AuthorizeResponse response, HttpRequestMessage request)
        {
            _response = response;
            _request = request;
        }

        public Task<HttpResponseMessage> ExecuteAsync(System.Threading.CancellationToken cancellationToken)
        {
            return Task.FromResult(Execute());
        }

        HttpResponseMessage Execute()
        {
            // TODO : cleanup using embedded assets helpers
            var root = _request.GetRequestContext().VirtualPathRoot;
            if (root.EndsWith("/")) root = root.Substring(0, root.Length - 1);
            string form = AssetManager.LoadResourceString("Thinktecture.IdentityServer.Core.Views.Embedded.Assets.app.FormPostResponse.html", new { rootUrl = root });
            form = form.Replace("{{redirect_uri}}", _response.RedirectUri.AbsoluteUri);

            var fields = _response.ToNameValueCollection();
            form = form.Replace("{{fields}}", fields.ToFormPost());

            var content = new StringContent(form, Encoding.UTF8, "text/html");
            var message = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = content
            };

            Logger.Info("Posting to " + _response.RedirectUri.AbsoluteUri);
            return message;
        }
    }
}