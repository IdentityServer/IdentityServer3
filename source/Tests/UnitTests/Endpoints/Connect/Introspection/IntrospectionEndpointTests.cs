using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Microsoft.Owin.Builder;
using FluentAssertions;
using System.Net;
using IdentityModel.Client;

namespace IdentityServer3.Tests.Endpoints.Connect.Introspection
{
    public class IntrospectionEndpointTests
    {
        const string Category = "Introspection endpoint";
        const string IntrospectionEndpoint = "https://server/connect/introspect";
        const string TokenEndpoint = "https://server/connect/token";

        private readonly HttpClient _client;
        private readonly OwinHttpMessageHandler _handler;

        public IntrospectionEndpointTests()
        {
            var app = Setup.IntrospectionIdentityServer.Create();
            _handler = new OwinHttpMessageHandler(app.Build());
            _client = new HttpClient(_handler);
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task Empty_Request()
        {
            var form = new Dictionary<string, string>();

            var response = await _client.PostAsync(IntrospectionEndpoint, new FormUrlEncodedContent(form));

            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task Unknown_Scope()
        {
            var form = new Dictionary<string, string>();

            _client.SetBasicAuthentication("unknown", "invalid");
            var response = await _client.PostAsync(IntrospectionEndpoint, new FormUrlEncodedContent(form));

            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task Invalid_ScopeSecret()
        {
            var form = new Dictionary<string, string>();

            _client.SetBasicAuthentication("api1", "invalid");
            var response = await _client.PostAsync(IntrospectionEndpoint, new FormUrlEncodedContent(form));

            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task Missing_Token()
        {
            var form = new Dictionary<string, string>();

            _client.SetBasicAuthentication("api1", "secret");
            var response = await _client.PostAsync(IntrospectionEndpoint, new FormUrlEncodedContent(form));

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task Invalid_Token()
        {
            var introspectionClient = new IntrospectionClient(
                IntrospectionEndpoint,
                "api1",
                "secret",
                _handler);

            var response = await introspectionClient.SendAsync(new IntrospectionRequest
            {
                Token = "invalid"
            });

            response.IsActive.Should().Be(false);
            response.IsError.Should().Be(false);
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task Valid_Token_Valid_Scope()
        {
            var tokenClient = new TokenClient(
                TokenEndpoint,
                "client1",
                "secret",
                _handler);

            var tokenResponse = await tokenClient.RequestClientCredentialsAsync("api1");

            var introspectionClient = new IntrospectionClient(
                IntrospectionEndpoint,
                "api1",
                "secret",
                _handler);

            var response = await introspectionClient.SendAsync(new IntrospectionRequest
            {
                Token = tokenResponse.AccessToken
            });

            response.IsActive.Should().Be(true);
            response.IsError.Should().Be(false);
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task Valid_Token_Valid_Scope_Multiple()
        {
            var tokenClient = new TokenClient(
                TokenEndpoint,
                "client1",
                "secret",
                _handler);

            var tokenResponse = await tokenClient.RequestClientCredentialsAsync("api1 api2");

            var introspectionClient = new IntrospectionClient(
                IntrospectionEndpoint,
                "api1",
                "secret",
                _handler);

            var response = await introspectionClient.SendAsync(new IntrospectionRequest
            {
                Token = tokenResponse.AccessToken
            });

            response.IsActive.Should().Be(true);
            response.IsError.Should().Be(false);
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task Valid_Token_Invalid_Scope()
        {
            var tokenClient = new TokenClient(
                TokenEndpoint,
                "client1",
                "secret",
                _handler);

            var tokenResponse = await tokenClient.RequestClientCredentialsAsync("api1");

            var introspectionClient = new IntrospectionClient(
                IntrospectionEndpoint,
                "api2",
                "secret",
                _handler);

            var response = await introspectionClient.SendAsync(new IntrospectionRequest
            {
                Token = tokenResponse.AccessToken
            });

            response.IsActive.Should().Be(false);
            response.IsError.Should().Be(false);
        }
    }
}