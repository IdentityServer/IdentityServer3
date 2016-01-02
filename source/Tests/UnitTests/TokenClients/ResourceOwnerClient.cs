using FluentAssertions;
using IdentityModel;
using IdentityModel.Client;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using Microsoft.Owin.Builder;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace IdentityServer3.Tests.TokenClients
{
    public class ResourceOwnerClient
    {
        const string TokenEndpoint = "https://server/connect/token";

        private readonly HttpClient _client;
        private readonly OwinHttpMessageHandler _handler;

        public ResourceOwnerClient()
        {
            var app = TokenClientIdentityServer.Create();
            _handler = new OwinHttpMessageHandler(app.Build());
            _client = new HttpClient(_handler);
        }

        [Fact]
        public async Task Valid_User()
        {
            var client = new TokenClient(
                TokenEndpoint,
                "roclient",
                "secret",
                innerHttpMessageHandler: _handler);

            var response = await client.RequestResourceOwnerPasswordAsync("bob", "bob", "api1");

            response.IsError.Should().Be(false);
            response.ExpiresIn.Should().Be(3600);
            response.TokenType.Should().Be("Bearer");
            response.IdentityToken.Should().BeNull();
            response.RefreshToken.Should().BeNull();

            var payload = GetPayload(response);

            payload.Count().Should().Be(10);
            payload.Should().Contain("iss", "https://idsrv3");
            payload.Should().Contain("aud", "https://idsrv3/resources");
            payload.Should().Contain("client_id", "roclient");
            payload.Should().Contain("scope", "api1");
            payload.Should().Contain("sub", "88421113");
            payload.Should().Contain("idp", "idsrv");

            var amr = payload["amr"] as JArray;
            amr.Count().Should().Be(1);
            amr.First().ToString().Should().Be("password");
        }

        [Fact]
        public async Task Valid_User_IdentityScopes()
        {
            var client = new TokenClient(
                TokenEndpoint,
                "roclient",
                "secret",
                innerHttpMessageHandler: _handler);

            var response = await client.RequestResourceOwnerPasswordAsync("bob", "bob", "openid email api1");

            response.IsError.Should().Be(false);
            response.ExpiresIn.Should().Be(3600);
            response.TokenType.Should().Be("Bearer");
            response.IdentityToken.Should().BeNull();
            response.RefreshToken.Should().BeNull();

            var payload = GetPayload(response);

            payload.Count().Should().Be(10);
            payload.Should().Contain("iss", "https://idsrv3");
            payload.Should().Contain("aud", "https://idsrv3/resources");
            payload.Should().Contain("client_id", "roclient");
            payload.Should().Contain("sub", "88421113");
            payload.Should().Contain("idp", "idsrv");

            var amr = payload["amr"] as JArray;
            amr.Count().Should().Be(1);
            amr.First().ToString().Should().Be("password");

            var scopes = payload["scope"] as JArray;
            scopes.Count().Should().Be(3);
            scopes.First().ToString().Should().Be("api1");
            scopes.Skip(1).First().ToString().Should().Be("email");
            scopes.Skip(2).First().ToString().Should().Be("openid");
        }

        [Fact]
        public async Task Valid_User_IdentityScopesRefreshToken()
        {
            var client = new TokenClient(
                TokenEndpoint,
                "roclient",
                "secret",
                innerHttpMessageHandler: _handler);

            var response = await client.RequestResourceOwnerPasswordAsync("bob", "bob", "openid email api1 offline_access");

            response.IsError.Should().Be(false);
            response.ExpiresIn.Should().Be(3600);
            response.TokenType.Should().Be("Bearer");
            response.IdentityToken.Should().BeNull();
            response.RefreshToken.Should().NotBeNullOrWhiteSpace();

            var payload = GetPayload(response);

            payload.Count().Should().Be(10);
            payload.Should().Contain("iss", "https://idsrv3");
            payload.Should().Contain("aud", "https://idsrv3/resources");
            payload.Should().Contain("client_id", "roclient");
            payload.Should().Contain("sub", "88421113");
            payload.Should().Contain("idp", "idsrv");

            var amr = payload["amr"] as JArray;
            amr.Count().Should().Be(1);
            amr.First().ToString().Should().Be("password");

            var scopes = payload["scope"] as JArray;
            scopes.Count().Should().Be(4);
            scopes.First().ToString().Should().Be("api1");
            scopes.Skip(1).First().ToString().Should().Be("email");
            scopes.Skip(2).First().ToString().Should().Be("offline_access");
            scopes.Skip(3).First().ToString().Should().Be("openid");
        }

        [Fact]
        public async Task Unknown_User()
        {
            var client = new TokenClient(
                TokenEndpoint,
                "roclient",
                "secret",
                innerHttpMessageHandler: _handler);

            var response = await client.RequestResourceOwnerPasswordAsync("unknown", "bob", "api1");

            response.IsError.Should().Be(true);
            response.Error.Should().Be("invalid_grant");
        }

        [Fact]
        public async Task Invalid_Password()
        {
            var client = new TokenClient(
                TokenEndpoint,
                "roclient",
                "secret",
                innerHttpMessageHandler: _handler);

            var response = await client.RequestResourceOwnerPasswordAsync("bob", "invalid", "api1");

            response.IsError.Should().Be(true);
            response.Error.Should().Be("invalid_grant");
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