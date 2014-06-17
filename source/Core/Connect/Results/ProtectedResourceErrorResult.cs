/*
 * Copyright (c) Dominick Baier, Brock Allen.  All rights reserved.
 * see license
 */

using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using Thinktecture.IdentityServer.Core.Logging;

namespace Thinktecture.IdentityServer.Core.Connect.Results
{
    public class ProtectedResourceErrorResult : IHttpActionResult
    {
        private readonly static ILog Logger = LogProvider.GetCurrentClassLogger();
        private readonly string _error;
        private readonly string _errorDescription;

        public ProtectedResourceErrorResult(string error, string errorDescription = null)
        {
            _error = error;
            _errorDescription = errorDescription;
        }

        public Task<HttpResponseMessage> ExecuteAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult<HttpResponseMessage>(Execute());
        }

        private HttpResponseMessage Execute()
        {
            var parameter = string.Format("error=\"{0}\"", _error);
            if (_errorDescription.IsPresent())
            {
                parameter = string.Format("{0}, error_description=\"{1}\"",
                    parameter, _errorDescription);
            }

            var header = new AuthenticationHeaderValue("Bearer", parameter);
            var response = new HttpResponseMessage(HttpStatusCode.Unauthorized);

            Logger.Info("Returning error: " + _error);
            return response;
        }
    }
}