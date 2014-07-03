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

namespace Thinktecture.IdentityServer.Core.Authentication
{
    public class LoginResult : IHttpActionResult
    {
        private readonly static ILog Logger = LogProvider.GetCurrentClassLogger();

        private readonly SignInMessage _message;
        private readonly HttpRequestMessage _request;
        private readonly CoreSettings _settings;
        private readonly InternalConfiguration _internalConfig;
        private string _loginPageUrl;

        public static string GetRedirectUrl(SignInMessage message, HttpRequestMessage request, CoreSettings settings, InternalConfiguration internalConfig)
        {
            var result = new LoginResult(message, request, settings, internalConfig, internalConfig.LoginPageUrl);
            var response = result.Execute();

            return response.Headers.Location.AbsoluteUri;
        }

        public LoginResult(SignInMessage message, HttpRequestMessage request, CoreSettings settings, InternalConfiguration internalConfig, string loginPageUrl = "")
        {
            _message = message;
            _settings = settings;
            _request = request;
            _internalConfig = internalConfig;
            _loginPageUrl = loginPageUrl;
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

                if (_loginPageUrl.IsMissing())
                {
                    var urlHelper = _request.GetUrlHelper();
                    _loginPageUrl = urlHelper.Route(Constants.RouteNames.Login, new { message = sim });
                }
                else
                {
                    _loginPageUrl += "?message=" + sim;
                }

                var uri = new Uri(_request.RequestUri, _loginPageUrl);

                response.Headers.Location = uri;
            }
            catch
            {
                response.Dispose();
                throw;
            }

            Logger.Info("Redirecting to login page");
            Logger.Debug(JsonConvert.SerializeObject(_message, Formatting.Indented));
            return response;
        }
    }
}