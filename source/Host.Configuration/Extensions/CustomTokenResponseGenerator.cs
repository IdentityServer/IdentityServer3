using IdentityServer3.Core.Models;
using IdentityServer3.Core.Services;
using IdentityServer3.Core.Validation;
using System.Threading.Tasks;

namespace Host.Configuration.Extensions
{
    class CustomTokenResponseGenerator : ICustomTokenResponseGenerator
    {
        public Task<TokenResponse> GenerateAsync(ValidatedTokenRequest request, TokenResponse response)
        {
            response.Custom.Add("custom_field", "custom data");
            response.Custom.Add("custom_complex_field", new ResponsePoco { SomeString = "foo", SomeInt = 42 });


            return Task.FromResult(response);
        }
    }

    class ResponsePoco
    {
        public string SomeString { get; set; }
        public int SomeInt { get; set; }
    }
}