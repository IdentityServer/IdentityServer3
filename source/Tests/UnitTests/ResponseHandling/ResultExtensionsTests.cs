using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using IdentityServer3.Core.Extensions;
using IdentityServer3.Core.Results;
using Xunit;

namespace IdentityServer3.Tests.ResponseHandling
{
    public class ResultExtensionsTests
    {
        public class TestController : ApiController
        {

        }

        [Fact]
        public void When_invoking_tokenErrorResponse_with_custom_result_parameters_then_result_contain_given_parameters()
        {
            var customResponseParameters = new Dictionary<string, object>()
            {
                {"customKey1", "customValue1"},
                {"customKey2", "customValue2"}
            };

            var result = new TestController().TokenErrorResponse("errorMessage", "description", customResponseParameters);

            var tokenErrorResult = result as TokenErrorResult;
            Assert.NotNull(tokenErrorResult);
            Assert.Equal(2, tokenErrorResult.CustomResponseParamaters.Count);
            Assert.NotNull(tokenErrorResult.CustomResponseParamaters.Any(x => x.Key == "customKey1" && x.Value == "customValue1"));
            Assert.NotNull(tokenErrorResult.CustomResponseParamaters.Any(x => x.Key == "customKey2" && x.Value == "customValue2"));
        }

        [Fact]
        public void When_invoking_tokenErrorResponse_without_custom_result_parameters_then_result_contain_empty_custom_response_parameters()
        {
            var result = new TestController().TokenErrorResponse("errorMessage", "description");

            var tokenErrorResult = result as TokenErrorResult;
            Assert.NotNull(tokenErrorResult);
            Assert.Empty(tokenErrorResult.CustomResponseParamaters);
        }
    }
}
