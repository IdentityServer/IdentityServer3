using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Owin.Builder;
using IdentityModel.Client;
using Xunit;
using FluentAssertions;

namespace IdentityServer3.Tests.TokenClients
{
    public class RefreshTokenClient
    {
        const string TokenEndpoint = "https://server/connect/token";
        const string IntrospectionEndpoint = "https://server/connect/introspect";

        const string clientId = "refresh.roclient.reference";
        const string clientSecret = "secret";

        const string scope = "api1";
        const string scopeSecret = "secret";


        private readonly HttpClient _client;
        private readonly OwinHttpMessageHandler _handler;

        public RefreshTokenClient()
        {
            var app = TokenClientIdentityServer.Create();
            _handler = new OwinHttpMessageHandler(app.Build());
            _client = new HttpClient(_handler);
        }

        [Fact]
        public async Task introspecting_same_access_token_twice_should_have_same_exp()
        {
            var tokenClient = new TokenClient(TokenEndpoint, clientId, clientSecret, _handler);
            var tokenResponse = await tokenClient.RequestResourceOwnerPasswordAsync("bob", "bob", "api1 offline_access");

            var introspectionClient = new IntrospectionClient(IntrospectionEndpoint, scope, scopeSecret, _handler);
            var introspectionResponse1 = await introspectionClient.SendAsync(new IntrospectionRequest
            {
                Token = tokenResponse.AccessToken
            });

            var introspectionResponse2 = await introspectionClient.SendAsync(new IntrospectionRequest
            {
                Token = tokenResponse.AccessToken
            });

            var exp1 = Int32.Parse(introspectionResponse1.Claims.Single(x => x.Item1 == "exp").Item2);
            var exp2 = Int32.Parse(introspectionResponse2.Claims.Single(x => x.Item1 == "exp").Item2);

            exp1.Should().Be(exp2);
        }

        [Fact]
        public async Task when_refreshing_new_exp_should_be_prior_to_old_exp()
        {
            var tokenClient = new TokenClient(TokenEndpoint, clientId, clientSecret, _handler);
            var tokenResponse = await tokenClient.RequestResourceOwnerPasswordAsync("bob", "bob", "api1 offline_access");

            var introspectionClient = new IntrospectionClient(IntrospectionEndpoint, scope, scopeSecret, _handler);
            var introspectionResponse1 = await introspectionClient.SendAsync(new IntrospectionRequest
            {
                Token = tokenResponse.AccessToken
            });
            var exp1 = Int32.Parse(introspectionResponse1.Claims.Single(x => x.Item1 == "exp").Item2);

            await Task.Delay(1000);

            var refreshResponse = await tokenClient.RequestRefreshTokenAsync(tokenResponse.RefreshToken);
            var introspectionResponse2 = await introspectionClient.SendAsync(new IntrospectionRequest
            {
                Token = refreshResponse.AccessToken
            });

            var exp2 = Int32.Parse(introspectionResponse2.Claims.Single(x => x.Item1 == "exp").Item2);

            exp1.Should().BeLessThan(exp2);
        }

        [Fact]
        public async Task when_refreshing_old_access_token_should_not_change_old_exp()
        {
            var tokenClient = new TokenClient(TokenEndpoint, clientId, clientSecret, _handler);
            var tokenResponse = await tokenClient.RequestResourceOwnerPasswordAsync("bob", "bob", "api1 offline_access");

            var introspectionClient = new IntrospectionClient(IntrospectionEndpoint, scope, scopeSecret, _handler);
            var introspectionResponse1 = await introspectionClient.SendAsync(new IntrospectionRequest
            {
                Token = tokenResponse.AccessToken
            });
            var exp1 = Int32.Parse(introspectionResponse1.Claims.Single(x => x.Item1 == "exp").Item2);

            await Task.Delay(1000);

            var refreshResponse = await tokenClient.RequestRefreshTokenAsync(tokenResponse.RefreshToken);
            var introspectionResponse2 = await introspectionClient.SendAsync(new IntrospectionRequest
            {
                Token = tokenResponse.AccessToken
            });

            var exp2 = Int32.Parse(introspectionResponse2.Claims.Single(x => x.Item1 == "exp").Item2);

            exp1.Should().Be(exp2);
        }
    }
}
