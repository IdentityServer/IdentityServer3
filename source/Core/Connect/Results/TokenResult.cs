/*
 * Copyright (c) Dominick Baier, Brock Allen.  All rights reserved.
 * see license
 */

using Newtonsoft.Json;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using Thinktecture.IdentityServer.Core.Connect.Models;
using Thinktecture.IdentityServer.Core.Logging;

namespace Thinktecture.IdentityServer.Core.Protocols.Connect.Results
{
    public class TokenResult : IHttpActionResult
    {
        private readonly static ILog Logger = LogProvider.GetCurrentClassLogger();
        private readonly TokenResponse _response;

        public TokenResult(TokenResponse response)
        {
            _response = response;
        }

        public Task<HttpResponseMessage> ExecuteAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult(Execute());
        }

        private HttpResponseMessage Execute()
        {
            var dto = new TokenResponseDto
            {
                id_token = _response.IdentityToken,
                access_token = _response.AccessToken,
                expires_in = _response.AccessTokenLifetime,
                token_type = Constants.TokenTypes.Bearer
            };

            var formatter = new JsonMediaTypeFormatter();
            formatter.SerializerSettings.DefaultValueHandling = DefaultValueHandling.Ignore;

            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new ObjectContent<TokenResponseDto>(dto, formatter)
            };

            Logger.Info("Returning token response.");
            return response;
        }

        internal class TokenResponseDto
        {
            public string id_token { get; set; }
            public string access_token { get; set; }
            public int expires_in { get; set; }
            public string token_type { get; set; }
        }    
    }
}