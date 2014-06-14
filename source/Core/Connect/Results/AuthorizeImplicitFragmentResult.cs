/*
 * Copyright (c) Dominick Baier, Brock Allen.  All rights reserved.
 * see license
 */

using System;
using System.Collections.Specialized;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using Thinktecture.IdentityServer.Core.Connect.Models;
using Thinktecture.IdentityServer.Core.Logging;

namespace Thinktecture.IdentityServer.Core.Connect.Results
{
    public class AuthorizeImplicitFragmentResult : IHttpActionResult
    {
        private readonly AuthorizeResponse _response;
        private readonly ILog _logger;

        public AuthorizeImplicitFragmentResult(AuthorizeResponse response)
        {
            _response = response;
            _logger = LogProvider.GetCurrentClassLogger();
        }

        public Task<HttpResponseMessage> ExecuteAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult<HttpResponseMessage>(Execute());
        }

        private HttpResponseMessage Execute()
        {
            var responseMessage = new HttpResponseMessage(HttpStatusCode.Redirect);
            var url = _response.RedirectUri.AbsoluteUri;
            var query = new NameValueCollection();

            if (_response.IdentityToken.IsPresent())
            {
                query.Add("id_token", _response.IdentityToken);
            }
            
            if (_response.AccessToken.IsPresent())
            {
                query.Add("access_token", _response.AccessToken);
                query.Add("token_type", "Bearer");
                query.Add("expires_in", _response.AccessTokenLifetime.ToString());
            }

            if (_response.Scope.IsPresent())
            {
                query.Add("scope", _response.Scope);
            }

            if (_response.State.IsPresent())
            {
                query.Add("state", _response.State);
            }

            url = string.Format("{0}#{1}", url, query.ToQueryString());
            responseMessage.Headers.Location = new Uri(url);
            
            _logger.Info("Redirecting to " + _response.RedirectUri.AbsoluteUri);
            
            return responseMessage;
        }
    }
}