using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using IdentityServer3.Core.Results;
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
    }
}
