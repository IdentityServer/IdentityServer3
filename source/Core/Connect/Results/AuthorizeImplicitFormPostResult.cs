/*
 * Copyright (c) Dominick Baier, Brock Allen.  All rights reserved.
 * see license
 */

using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using Thinktecture.IdentityServer.Core.Connect.Models;
using Thinktecture.IdentityServer.Core.Logging;

namespace Thinktecture.IdentityServer.Core.Connect.Results
{
    public class AuthorizeImplicitFormPostResult : IHttpActionResult
    {
        private readonly static ILog Logger = LogProvider.GetCurrentClassLogger();
        private readonly AuthorizeResponse _response;

        public AuthorizeImplicitFormPostResult(AuthorizeResponse response)
        {
            _response = response;
        }

        public Task<HttpResponseMessage> ExecuteAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult<HttpResponseMessage>(Execute());
        }

        private HttpResponseMessage Execute()
        {
            string form;
            using (var stream = this.GetType().Assembly.GetManifestResourceStream("Thinktecture.IdentityServer.Core.Connect.FormPostResponse.html"))
            {
                using (var reader = new StreamReader(stream))
                {
                    form = reader.ReadToEnd();
                }
            }

            form = form.Replace("{{redirect_uri}}", _response.RedirectUri.AbsoluteUri);

            var sb = new StringBuilder(128);
            var inputFieldFormat = "<input type=\"hidden\" name=\"{0}\" value=\"{1}\" />\n";

            if (_response.IdentityToken.IsPresent())
            {
                sb.AppendFormat(inputFieldFormat, "id_token", _response.IdentityToken);
            }

            if (_response.AccessToken.IsPresent())
            {
                sb.AppendFormat(inputFieldFormat, "token", _response.AccessToken);
                sb.AppendFormat(inputFieldFormat, "expires_in", _response.AccessTokenLifetime);
            }
            
            if (_response.State.IsPresent())
            {
                sb.AppendFormat(inputFieldFormat, "state", _response.State);
            }

            form = form.Replace("{{fields}}", sb.ToString());

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