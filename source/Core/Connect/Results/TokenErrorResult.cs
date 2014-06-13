/*
 * Copyright (c) Dominick Baier, Brock Allen.  All rights reserved.
 * see license
 */

using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using Thinktecture.IdentityServer.Core.Logging;

namespace Thinktecture.IdentityServer.Core.Connect.Results
{
    public class TokenErrorResult : IHttpActionResult
    {
        private readonly string _error;
        private readonly ILog _logger;

        public TokenErrorResult(string error)
        {
            _error = error;
            _logger = LogProvider.GetCurrentClassLogger();
        }

        public Task<HttpResponseMessage> ExecuteAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult<HttpResponseMessage>(Execute());
        }

        private HttpResponseMessage Execute()
        {
            var dto = new ErrorDto
            {
                error = _error 
            };

            HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.BadRequest)
            {
                Content = new ObjectContent<ErrorDto>(dto, new JsonMediaTypeFormatter())
            };

            _logger.Info("Returning error: " + _error);
            return response;
        }

        internal class ErrorDto
        {
            public string error { get; set; }
        }    
    }
}