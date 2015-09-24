/*
 * Copyright 2014 Dominick Baier, Brock Allen
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *   http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using FluentAssertions;
using IdentityServer3.Core.Validation;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace IdentityServer3.Tests.Validation
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