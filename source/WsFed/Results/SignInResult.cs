/*
 * Copyright (c) Dominick Baier, Brock Allen.  All rights reserved.
 * see license
 */
using System.IdentityModel.Services;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using Thinktecture.IdentityServer.Core.Logging;

namespace Thinktecture.IdentityServer.WsFederation.Results
{
    public class SignInResult : IHttpActionResult
    {
        private readonly static ILog Logger = LogProvider.GetCurrentClassLogger();
        private readonly SignInResponseMessage _message;

        public SignInResult(SignInResponseMessage message)
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
            response.Content = new StringContent(_message.WriteFormPost(), Encoding.UTF8, "text/html");

            Logger.Debug("Returning WS-Federation signin response");
            return response;
        }
    }
}