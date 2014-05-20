/*
 * Copyright (c) Dominick Baier, Brock Allen.  All rights reserved.
 * see license
 */

using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Thinktecture.IdentityServer.Core.Connect.Models;

namespace Thinktecture.IdentityServer.Core.Connect.Results
{
    public class AuthorizeImplicitJsonResult : IHttpActionResult
    {
        private readonly AuthorizeResponse _response;

        public AuthorizeImplicitJsonResult(AuthorizeResponse response)
        {
            _response = response;
        }

        public Task<HttpResponseMessage> ExecuteAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult<HttpResponseMessage>(Execute());
        }

        private HttpResponseMessage Execute()
        {
            var json = new JObject();

            json["redirect_uri"] = _response.RedirectUri.AbsoluteUri;
           
            if (_response.IdentityToken.IsPresent())
            {
                json["id_token"] = _response.IdentityToken;
            }

            if (_response.AccessToken.IsPresent())
            {
                json["token"] = _response.AccessToken;
                json["expires_in"] = _response.AccessTokenLifetime;
            }
            
            if (_response.State.IsPresent())
            {
                json["state"] = _response.State;
            }            

            var content = new StringContent(JsonConvert.SerializeObject(json), Encoding.UTF8, "application/json");
            var message = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = content
            };

            return message;
        }
    }
}