using FluentAssertions;
using IdentityModel.Client;
using Microsoft.Owin.Builder;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace IdentityServer3.Tests.TokenClients
{ 
    public class UserInfoEndpointClient
    {
        const string TokenEndpoint = "https://server/connect/token";
        const string UserInfoEndpoint = "https://server/connect/userinfo";

        private readonly HttpClient _client;
        private readonly HttpMessageHandler _handler;

        public UserInfoEndpointClient()
        {
            var app = TokenClientIdentityServer.Create();
            _handler = new OwinHttpMessageHandler(app.Build());
            _client = new HttpClient(_handler);
        }

        [Fact]
        public async Task Valid_Client()
        {
            var tokenClient = new TokenClient(
                TokenEndpoint,
                "roclient",
                "secret",
                innerHttpMessageHandler: _handler);

            var response = await tokenClient.RequestResourceOwnerPasswordAsync("bob", "bob", "openid email api1");
            response.IsError.Should().BeFalse();

            var userInfoclient = new UserInfoClient(
                new Uri(UserInfoEndpoint),
                response.AccessToken,
                _handler);

            var userInfo = await userInfoclient.GetAsync();

            userInfo.IsError.Should().BeFalse();
            userInfo.Claims.Count().Should().Be(3);
            userInfo.Claims.Should().Contain(Tuple.Create("sub", "88421113"));
            userInfo.Claims.Should().Contain(Tuple.Create("email", "BobSmith@email.com"));
            userInfo.Claims.Should().Contain(Tuple.Create("email_verified", "True"));
        }

        [Fact]
        public async Task Address_Scope()
        {
            var tokenClient = new TokenClient(
                TokenEndpoint,
                "roclient",
                "secret",
                innerHttpMessageHandler: _handler);

            var response = await tokenClient.RequestResourceOwnerPasswordAsync("bob", "bob", "openid address");
            response.IsError.Should().BeFalse();

            var userInfoclient = new UserInfoClient(
                new Uri(UserInfoEndpoint),
                response.AccessToken,
                _handler);

            var userInfo = await userInfoclient.GetAsync();

            userInfo.IsError.Should().BeFalse();
            userInfo.Raw.Should().Be("{\"sub\":\"88421113\",\"address\":{\"street_address\":\"One Hacker Way\",\"locality\":\"Heidelberg\",\"postal_code\":69118,\"country\":\"Germany\"}}");
        }

        [Fact]
        public async Task No_Identity_Scope()
        {
            var tokenClient = new TokenClient(
                TokenEndpoint,
                "roclient",
                "secret",
                innerHttpMessageHandler: _handler);

            var response = await tokenClient.RequestResourceOwnerPasswordAsync("bob", "bob", "api1");
            response.IsError.Should().BeFalse();

            var userInfoclient = new UserInfoClient(
                new Uri(UserInfoEndpoint),
                response.AccessToken,
                _handler);

            var userInfo = await userInfoclient.GetAsync();

            userInfo.IsError.Should().BeTrue();
            userInfo.HttpErrorStatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task Identity_Scope_No_OpenID()
        {
            var tokenClient = new TokenClient(
                TokenEndpoint,
                "roclient",
                "secret",
                innerHttpMessageHandler: _handler);

            var response = await tokenClient.RequestResourceOwnerPasswordAsync("bob", "bob", "email api1");
            response.IsError.Should().BeFalse();

            var userInfoclient = new UserInfoClient(
                new Uri(UserInfoEndpoint),
                response.AccessToken,
                _handler);

            var userInfo = await userInfoclient.GetAsync();

            userInfo.IsError.Should().BeTrue();
            userInfo.HttpErrorStatusCode.Should().Be(HttpStatusCode.Forbidden);
        }
    }
}