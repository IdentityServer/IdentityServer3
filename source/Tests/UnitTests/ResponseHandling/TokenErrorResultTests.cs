using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using IdentityServer3.Core.Results;
using Newtonsoft.Json.Linq;
using Xunit;

namespace IdentityServer3.Tests.ResponseHandling
{
    public class TokenErrorResultTests
    {
        [Fact]
        public async Task When_error_result_contains_custom_response_parameters_then_execute_return_json_with_given_parameters()
        {
            var customResponseParameters = new Dictionary<string, object>()
            {
                {"customKey1", "customValue1"},
                {"customKey2", "customValue2"}
            };

            var errorResult = new TokenErrorResult("error message", "error description", customResponseParameters);
            var result = await errorResult.ExecuteAsync(new CancellationToken());

            JavaScriptSerializer jsonSerializer = new JavaScriptSerializer();
            var content = await result.Content.ReadAsStringAsync();
            var contentObject = (IDictionary<string, object>)jsonSerializer.DeserializeObject(content);

            Assert.NotNull(contentObject);
            Assert.True(contentObject.ContainsKey("customKey1"));
            Assert.Equal("customValue1", contentObject["customKey1"]);
            Assert.True(contentObject.ContainsKey("customKey2"));
            Assert.Equal("customValue2", contentObject["customKey2"]);
        }

        [Fact]
        public async Task When_error_result_contains_custom_response_parameters_with_existing_key_then_execut_throw_exception()
        {
            var customResponseParameters = new Dictionary<string, object>()
            {
                {"error", "another error value"}
            };

            var errorResult = new TokenErrorResult("error message", "error description", customResponseParameters);

            await Assert.ThrowsAsync<System.Exception>(async () =>
            {
                await errorResult.ExecuteAsync(new CancellationToken());
            });
        }

        [Fact]
        public void When_invoking_tokenErrorResponse_with_custom_result_parameters_then_result_contain_given_parameters()
        {
            // Given 
            var customResponseParameters = new Dictionary<string, object>()
            {
                {"customKey1", "customValue1"},
                {"customKey2", "customValue2"}
            };

            // When
            var result = new TokenErrorResult("errorMessage", "description", customResponseParameters);

            // Then
            Assert.NotNull(result);
            Assert.Equal(2, result.CustomResponseParamaters.Count);
            Assert.NotNull(result.CustomResponseParamaters.Any(x => x.Key == "customKey1" && x.Value == "customValue1"));
            Assert.NotNull(result.CustomResponseParamaters.Any(x => x.Key == "customKey2" && x.Value == "customValue2"));
        }

        [Fact]
        public void When_invoking_tokenErrorResponse_without_custom_result_parameters_then_result_contain_empty_custom_response_parameters()
        {
            // When
            var result = new TokenErrorResult("errorMessage", "description");

            // Then
            Assert.NotNull(result);
            Assert.Empty(result.CustomResponseParamaters);
        }

        [Fact]
        public async Task When_invoking_execute_with_custom_parameters_then_content_contain_proper_custom_parameters()
        {
            // Given 
            var customResponseParameters = new Dictionary<string, object>()
            {
                {"customKey1", "customValue1"},
                {"customKey2", "customValue2"}
            };

            // When
            var errorToken = new TokenErrorResult("errorMessage", "description", customResponseParameters);

            var result = await errorToken.ExecuteAsync(new CancellationToken());
            var resultContent = await result.Content.ReadAsStringAsync();
            var parsedContent = JObject.Parse(resultContent);

            // Then
            var customKey1 = parsedContent.GetValue("customKey1");
            Assert.Equal("customValue1", customKey1.Value<string>());

            var customKey2 = parsedContent.GetValue("customKey2");
            Assert.Equal("customValue2", customKey2.Value<string>());
        }

        [Fact]
        public async Task When_invoking_execute_with_custom_parameters_then_content_is_in_proper_json_format()
        {
            // Given 
            var customResponseParameters = new Dictionary<string, object>()
            {
                {"customKey1", "customValue1"},
                {"customKey2", "customValue2"}
            };

            // When
            var errorToken = new TokenErrorResult("errorMessage", "description", customResponseParameters);

            var result = await errorToken.ExecuteAsync(new CancellationToken());
            var resultContent = await result.Content.ReadAsStringAsync();
            var parsedContent = JObject.Parse(resultContent);

            // Then
            Assert.NotNull(parsedContent);
        }

        [Fact]
        public async Task When_invoking_execute_with_null_custom_parameters_then_does_not_throw_exception()
        {
            // When
            var errorToken = new TokenErrorResult("errorMessage", "description", null);

            var result = await errorToken.ExecuteAsync(new CancellationToken());
            var resultContent = await result.Content.ReadAsStringAsync();

            // Then
            Assert.NotNull(resultContent);
        }

        [Fact]
        public async Task When_invoking_execute_then_httpStatusCode_is_400()
        {
            // When
            var errorToken = new TokenErrorResult("errorMessage", "description");

            var result = await errorToken.ExecuteAsync(new CancellationToken());

            // Then
            Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
        }
    }
}
