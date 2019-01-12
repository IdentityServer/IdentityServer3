using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using IdentityServer3.Core.Models;
using IdentityServer3.Core.Results;
using Xunit;

namespace IdentityServer3.Tests.ResponseHandling
{
    public class TokenResultTests
    {
        [Fact]
        public async Task When_creating_token_response_with_custom_response_parameters_then_result_contain_given_parameters()
        {
            // Given
            var tokenResult = new TokenResult(new TokenResponse()
            {
                Custom = new Dictionary<string, object>()
                {
                    {"customParameter1", "customValue1"}
                }
            });
            JavaScriptSerializer jsonSerializer = new JavaScriptSerializer();

            // When
            var result = await tokenResult.ExecuteAsync(new CancellationToken());
            var content = await result.Content.ReadAsStringAsync();
            var contentObject = (IDictionary<string, object>)jsonSerializer.DeserializeObject(content);

            // Then
            Assert.True(contentObject.ContainsKey("customParameter1"));
            Assert.Equal("customValue1", contentObject["customParameter1"]);
        }
    }
}
