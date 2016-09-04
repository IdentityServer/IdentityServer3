using System.Collections.Generic;
using System.Web.Http;
using IdentityServer3.Core.Results;
using Xunit;
using IdentityServer3.Core.Extensions;
using IdentityServer3.Core.Models;

namespace IdentityServer3.Tests.ResponseHandling
{
    public class TokenExtensionsTests
    {
        public class TestController : ApiController
        {


        }

        [Fact]
        public void When_creating_token_error_response_with_error_then_result_is_instance_of_tokenErrorResult()
        {
            // When
            var tokenResponse = new TestController().TokenErrorResponse("error");

            // THen
            Assert.NotNull(tokenResponse as TokenErrorResult);
        }

        [Fact]
        public void When_creating_token_error_response_with_error_then_result_contain_error()
        {
            // When
            var tokenResponse = new TestController().TokenErrorResponse("error") as TokenErrorResult;

            // THen
            Assert.Equal("error", tokenResponse.Error);
        }

        [Fact]
        public void When_creating_token_error_response_with_error_and_description_then_result_contain_given_data()
        {
            // When
            var tokenResponse = new TestController().TokenErrorResponse("error", "description") as TokenErrorResult;

            // THen
            Assert.Equal("error", tokenResponse.Error);
            Assert.Equal("description", tokenResponse.ErrorDescription);
        }

        [Fact]
        public void When_creating_token_error_response_with_error_description_and_custom_parameters_then_result_contain_given_data()
        {
            // When
            var tokenResponse = new TestController().TokenErrorResponse("error", "description", new Dictionary<string, object>()
            {
                {"customParameter1","customValue1" }
            }) as TokenErrorResult;

            // Then
            Assert.Equal("error", tokenResponse.Error);
            Assert.Equal("description", tokenResponse.ErrorDescription);
            Assert.Equal(1, tokenResponse.CustomResponseParamaters.Count);
            Assert.Equal("customValue1", tokenResponse.CustomResponseParamaters["customParameter1"]);
        }
    }
}
