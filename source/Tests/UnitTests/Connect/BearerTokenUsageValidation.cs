using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using FluentAssertions;
using Thinktecture.IdentityServer.Core.Validation;
using Xunit;

namespace Thinktecture.IdentityServer.Tests.Connect
{
    public class BearerTokenUsageValidation
    {
        const string Category = "BearerTokenUsageValidator Tests";

        [Fact]
        [Trait("Category", Category)]
        public async Task No_Header_no_Body_Get()
        {
            var request = new HttpRequestMessage();
            request.Method = HttpMethod.Get;

            var validator = new BearerTokenUsageValidator();
            var result = await validator.ValidateAsync(request);

            result.TokenFound.Should().BeFalse();
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task No_Header_no_Body_Post()
        {
            var request = new HttpRequestMessage();
            request.Method = HttpMethod.Post;
            request.Content = new FormUrlEncodedContent(new Dictionary<string, string>());

            var validator = new BearerTokenUsageValidator();
            var result = await validator.ValidateAsync(request);

            result.TokenFound.Should().BeFalse();
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task Non_Bearer_Scheme_Header()
        {
            var request = new HttpRequestMessage();
            request.Method = HttpMethod.Get;
            request.Headers.Add("Authorization", "Foo Bar");

            var validator = new BearerTokenUsageValidator();
            var result = await validator.ValidateAsync(request);

            result.TokenFound.Should().BeFalse();
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task Empty_Bearer_Scheme_Header()
        {
            var request = new HttpRequestMessage();
            request.Method = HttpMethod.Get;
            request.Headers.Add("Authorization", "Bearer");

            var validator = new BearerTokenUsageValidator();
            var result = await validator.ValidateAsync(request);

            result.TokenFound.Should().BeFalse();
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task Whitespaces_Bearer_Scheme_Header()
        {
            var request = new HttpRequestMessage();
            request.Method = HttpMethod.Get;
            request.Headers.Add("Authorization", "Bearer           ");

            var validator = new BearerTokenUsageValidator();
            var result = await validator.ValidateAsync(request);

            result.TokenFound.Should().BeFalse();
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task Valid_Bearer_Scheme_Header()
        {
            var request = new HttpRequestMessage();
            request.Method = HttpMethod.Get;
            request.Headers.Add("Authorization", "Bearer token");

            var validator = new BearerTokenUsageValidator();
            var result = await validator.ValidateAsync(request);

            result.TokenFound.Should().BeTrue();
            result.Token.Should().Be("token");
            result.UsageType.Should().Be(BearerTokenUsageType.AuthorizationHeader);
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task Valid_Body_Post()
        {
            var request = new HttpRequestMessage();
            request.Method = HttpMethod.Post;
            request.Content = new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    { "access_token", "token" }
                });

            var validator = new BearerTokenUsageValidator();
            var result = await validator.ValidateAsync(request);

            result.TokenFound.Should().BeTrue();
            result.Token.Should().Be("token");
            result.UsageType.Should().Be(BearerTokenUsageType.PostBody);
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task Body_Post_empty_Token()
        {
            var request = new HttpRequestMessage();
            request.Method = HttpMethod.Post;
            request.Content = new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    { "access_token", "" }
                });

            var validator = new BearerTokenUsageValidator();
            var result = await validator.ValidateAsync(request);

            result.TokenFound.Should().BeFalse();
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task Body_Post_Whitespace_Token()
        {
            var request = new HttpRequestMessage();
            request.Method = HttpMethod.Post;
            request.Content = new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    { "access_token", "    " }
                });

            var validator = new BearerTokenUsageValidator();
            var result = await validator.ValidateAsync(request);

            result.TokenFound.Should().BeFalse();
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task Body_Post_no_Token()
        {
            var request = new HttpRequestMessage();
            request.Method = HttpMethod.Post;
            request.Content = new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    { "foo", "bar" }
                });

            var validator = new BearerTokenUsageValidator();
            var result = await validator.ValidateAsync(request);

            result.TokenFound.Should().BeFalse();
        }
    }
}