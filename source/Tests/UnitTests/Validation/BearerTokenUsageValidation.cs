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
using Microsoft.Owin;
using System.IO;
using System.Text;
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
            var ctx = new OwinContext();
            ctx.Request.Method = "GET";

            var validator = new BearerTokenUsageValidator();
            var result = await validator.ValidateAsync(ctx);

            result.TokenFound.Should().BeFalse();
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task No_Header_no_Body_Post()
        {
            var ctx = new OwinContext();
            ctx.Request.Method = "POST";

            var validator = new BearerTokenUsageValidator();
            var result = await validator.ValidateAsync(ctx);

            result.TokenFound.Should().BeFalse();
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task Non_Bearer_Scheme_Header()
        {
            var ctx = new OwinContext();
            ctx.Request.Method = "GET";
            ctx.Request.Headers.Add("Authorization", new string[] { "Foo Bar" });

            var validator = new BearerTokenUsageValidator();
            var result = await validator.ValidateAsync(ctx);

            result.TokenFound.Should().BeFalse();
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task Empty_Bearer_Scheme_Header()
        {
            var ctx = new OwinContext();
            ctx.Request.Method = "GET";
            ctx.Request.Headers.Add("Authorization", new string[] { "Bearer" });

            var validator = new BearerTokenUsageValidator();
            var result = await validator.ValidateAsync(ctx);

            result.TokenFound.Should().BeFalse();
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task Whitespaces_Bearer_Scheme_Header()
        {
            var ctx = new OwinContext();
            ctx.Request.Method = "GET";
            ctx.Request.Headers.Add("Authorization", new string[] { "Bearer           " });

            var validator = new BearerTokenUsageValidator();
            var result = await validator.ValidateAsync(ctx);

            result.TokenFound.Should().BeFalse();
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task Valid_Bearer_Scheme_Header()
        {
            var ctx = new OwinContext();
            ctx.Request.Method = "GET";
            ctx.Request.Headers.Add("Authorization", new string[] { "Bearer token" });

            var validator = new BearerTokenUsageValidator();
            var result = await validator.ValidateAsync(ctx);

            result.TokenFound.Should().BeTrue();
            result.Token.Should().Be("token");
            result.UsageType.Should().Be(BearerTokenUsageType.AuthorizationHeader);
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task Valid_Body_Post()
        {
            var ctx = new OwinContext();
            ctx.Request.Method = "POST";
            ctx.Request.ContentType = "application/x-www-form-urlencoded";
            var body = "access_token=token";
            ctx.Request.Body = new MemoryStream(Encoding.UTF8.GetBytes(body));

            var validator = new BearerTokenUsageValidator();
            var result = await validator.ValidateAsync(ctx);

            result.TokenFound.Should().BeTrue();
            result.Token.Should().Be("token");
            result.UsageType.Should().Be(BearerTokenUsageType.PostBody);
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task Body_Post_empty_Token()
        {
            var ctx = new OwinContext();
            ctx.Request.Method = "POST";
            ctx.Request.ContentType = "application/x-www-form-urlencoded";
            var body = "access_token=";
            ctx.Request.Body = new MemoryStream(Encoding.UTF8.GetBytes(body));

            var validator = new BearerTokenUsageValidator();
            var result = await validator.ValidateAsync(ctx);

            result.TokenFound.Should().BeFalse();
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task Body_Post_Whitespace_Token()
        {
            var ctx = new OwinContext();
            ctx.Request.Method = "POST";
            ctx.Request.ContentType = "application/x-www-form-urlencoded";
            var body = "access_token=                ";
            ctx.Request.Body = new MemoryStream(Encoding.UTF8.GetBytes(body));

            var validator = new BearerTokenUsageValidator();
            var result = await validator.ValidateAsync(ctx);

            result.TokenFound.Should().BeFalse();
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task Body_Post_no_Token()
        {
            var ctx = new OwinContext();
            ctx.Request.Method = "POST";
            ctx.Request.ContentType = "application/x-www-form-urlencoded";
            var body = "foo=bar";
            ctx.Request.Body = new MemoryStream(Encoding.UTF8.GetBytes(body));

            var validator = new BearerTokenUsageValidator();
            var result = await validator.ValidateAsync(ctx);

            result.TokenFound.Should().BeFalse();
        }
    }
}