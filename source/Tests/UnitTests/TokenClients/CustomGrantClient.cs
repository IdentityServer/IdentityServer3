using FluentAssertions;
using IdentityModel;
using IdentityModel.Client;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using Microsoft.Owin.Builder;
using System.Threading.Tasks;
using Xunit;

namespace IdentityServer3.Tests.TokenClients
{
    public class CustomGrantClient
    {
        const string TokenEndpoint = "https://server/connect/token";

        private readonly HttpClient _client;
        private readonly OwinHttpMessageHandler _handler;

        public CustomGrantClient()
        {
            var app = TokenClientIdentityServer.Create();
            _handler = new OwinHttpMessageHandler(app.Build());
            _client = new HttpClient(_handler);
        }

        [Fact]
        public async Task Valid_Client()
        {
            var client = new TokenClient(
                TokenEndpoint,
                "client.custom",
                "secret",
                innerHttpMessageHandler: _handler);

            var customParameters = new Dictionary<string, string>
                {
                    { "custom_credential", "custom credential"}
                };

            var response = await client.RequestCustomGrantAsync("custom", "api1", customParameters);

            response.IsError.Should().Be(false);
            response.ExpiresIn.Should().Be(3600);
            response.TokenType.Should().Be("Bearer");
            response.IdentityToken.Should().BeNull();
            response.RefreshToken.Should().BeNull();

            var payload = GetPayload(response);

            payload.Count().Should().Be(10);
            payload.Should().Contain("iss", "https://idsrv3");
            payload.Should().Contain("aud", "https://idsrv3/resources");
            payload.Should().Contain("client_id", "client.custom");
            payload.Should().Contain("scope", "api1");
            payload.Should().Contain("sub", "818727");
            payload.Should().Contain("idp", "idsrv");

            var amr = payload["amr"] as JArray;
            amr.Count().Should().Be(1);
            amr.First().ToString().Should().Be("custom");
        }

        [Fact]
        public async Task Valid_Client_Missing_Grant_Specific_Data()
        {
            var client = new TokenClient(
                TokenEndpoint,
                "client.custom",
                "secret",
                innerHttpMessageHandler: _handler);

            var response = await client.RequestCustomGrantAsync("custom", "api1");

            response.IsError.Should().Be(true);
            response.Error.Should().Be("invalid_custom_credential");
        }

        [Fact]
        public async Task Valid_Client_Unsupported_Grant()
        {
            var client = new TokenClient(
                TokenEndpoint,
                "client.custom",
                "secret",
                innerHttpMessageHandler: _handler);

            var customParameters = new Dictionary<string, string>
                {
                    { "custom_credential", "custom credential"}
                };

            var response = await client.RequestCustomGrantAsync("invalid", "api1", customParameters);

            response.IsError.Should().Be(true);
            response.Error.Should().Be("unsupported_grant_type");
        }

        [Fact]
        public async Task Valid_Client_Unauthorized_Grant()
        {
            var client = new TokenClient(
                TokenEndpoint,
                "client.custom",
                "secret",
                innerHttpMessageHandler: _handler);

            var customParameters = new Dictionary<string, string>
                {
                    { "custom_credential", "custom credential"}
                };

            var response = await client.RequestCustomGrantAsync("custom2", "api1", customParameters);

            response.IsError.Should().Be(true);
            response.Error.Should().Be("unsupported_grant_type");
        }

        private Dictionary<string, object> GetPayload(TokenResponse response)
        {
            var token = response.AccessToken.Split('.').Skip(1).Take(1).First();
            var dictionary = JsonConvert.DeserializeObject<Dictionary<string, object>>(
                Encoding.UTF8.GetString(Base64Url.Decode(token)));

            return dictionary;
        }
    }
}