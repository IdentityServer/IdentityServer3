/*
 * Copyright (c) Dominick Baier, Brock Allen.  All rights reserved.
 * see license
 */

using Newtonsoft.Json;
using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using Thinktecture.IdentityServer.Core.Configuration;
using Thinktecture.IdentityServer.Core.Logging;
using Thinktecture.IdentityServer.Core.Models;

namespace Thinktecture.IdentityServer.Core.Authentication
{
    public class LoginResult : IHttpActionResult
    {
        private readonly SignInMessage _message;
        private readonly HttpRequestMessage _request;
        private readonly CoreSettings _settings;
        private readonly InternalConfiguration _internalConfig;
        private readonly ILog _logger;

        public LoginResult(SignInMessage message, HttpRequestMessage request, CoreSettings settings, InternalConfiguration internalConfig)
        {
            _message = message;
            _settings = settings;
            _request = request;
            _internalConfig = internalConfig;
            _logger = LogProvider.GetCurrentClassLogger();
        }

        public Task<HttpResponseMessage> ExecuteAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult(Execute());
        }

        private HttpResponseMessage Execute()
        {
            HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.Redirect);

            try
            {
                var sim = _message.Protect(600, _internalConfig.DataProtector);

                var urlHelper = _request.GetUrlHelper();
                var loginUrl = urlHelper.Route(Constants.RouteNames.Login, new { message = sim });
                var uri = new Uri(_request.RequestUri, loginUrl);

                response.Headers.Location = uri;
            }
            catch
            {
                response.Dispose();
                throw;
            }

            _logger.Info("Redirecting to login page");
            _logger.Debug(JsonConvert.SerializeObject(_message, Formatting.Indented));
            return response;
        }
    }
}