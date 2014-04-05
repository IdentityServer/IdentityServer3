/*
 * Copyright (c) Dominick Baier, Brock Allen.  All rights reserved.
 * see license
 */

using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using Thinktecture.IdentityServer.Core.Connect.Models;

namespace Thinktecture.IdentityServer.Core.Connect.Results
{
    public class AuthorizeCodeResult : IHttpActionResult
    {
        private readonly AuthorizeResponse _response;

        public AuthorizeCodeResult(AuthorizeResponse response)
        {
            _response = response;
        }

        public Task<HttpResponseMessage> ExecuteAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult<HttpResponseMessage>(Execute());
        }

        private HttpResponseMessage Execute()
        {
            var responseMessage = new HttpResponseMessage(HttpStatusCode.Redirect);

            var url = string.Format("{0}?code={1}", _response.RedirectUri.AbsoluteUri, _response.Code);

            if (_response.State.IsPresent())
            {
                url = string.Format("{0}&state={1}", url, _response.State);
            }

            responseMessage.Headers.Location = new Uri(url);
            return responseMessage;
        }
    }
}