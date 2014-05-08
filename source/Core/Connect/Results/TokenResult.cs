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

namespace Thinktecture.IdentityServer.Core.Protocols.Connect.Results
{
    public class TokenResult : IHttpActionResult
    {
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
                IdToken = _response.IdentityToken,
                AccessToken = _response.AccessToken,
                ExpiresIn = _response.AccessTokenLifetime,
                TokenType = Constants.TokenTypes.Bearer
            };

            var formatter = new JsonMediaTypeFormatter();
            formatter.SerializerSettings.DefaultValueHandling = DefaultValueHandling.Ignore;

            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new ObjectContent<TokenResponseDto>(dto, formatter)
            };

            return response;
        }

        internal class TokenResponseDto
        {
			[JsonProperty("id_token")]
            public string IdToken { get; set; }

			[JsonProperty("access_token")]
            public string AccessToken { get; set; }

			[JsonProperty("expires_in")]
            public int ExpiresIn { get; set; }

			[JsonProperty("token_type")]
            public string TokenType { get; set; }
        }    
    }
}