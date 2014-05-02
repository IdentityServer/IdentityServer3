using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Infrastructure;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;

namespace IdSrvReferenceTokenValidation
{
    public class IdSrvReferenceTokenProvider : AuthenticationTokenProvider
    {
        private HttpClient _client;
        private string _tokenValidationEndpoint;

        public IdSrvReferenceTokenProvider(string tokenValidationEndpoint)
        {
            _tokenValidationEndpoint = tokenValidationEndpoint + "?token={0}";
            _client = new HttpClient();
        }

        public override async Task ReceiveAsync(AuthenticationTokenReceiveContext context)
        {
            var url = string.Format(_tokenValidationEndpoint, context.Token);
            
            var response = await _client.GetAsync(url);
            if (response.StatusCode != HttpStatusCode.OK)
            {
                return;
            }

            var json = JArray.Parse(await response.Content.ReadAsStringAsync());
            var claims = new List<Claim>();

            foreach (var item in json)
            {
                claims.Add(new Claim(item["Type"].ToString(), item["Value"].ToString()));
            }

            context.SetTicket(new AuthenticationTicket(new ClaimsIdentity(claims, "idsrv"), new AuthenticationProperties()));
        }
    }
}