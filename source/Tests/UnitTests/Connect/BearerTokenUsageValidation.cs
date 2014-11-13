using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Thinktecture.IdentityServer.Core.Validation;

namespace Thinktecture.IdentityServer.Tests.Connect
{
    [TestClass]
    public class BearerTokenUsageValidation
    {
        const string Category = "BearerTokenUsageValidator Tests";

        [TestMethod]
        [TestCategory(Category)]
        public async Task No_Header_no_Body_Get()
        {
            var request = new HttpRequestMessage();
            request.Method = HttpMethod.Get;

            var validator = new BearerTokenUsageValidator();
            var result = await validator.ValidateAsync(request);

            Assert.IsFalse(result.TokenFound);
        }

        [TestMethod]
        [TestCategory(Category)]
        public async Task No_Header_no_Body_Post()
        {
            var request = new HttpRequestMessage();
            request.Method = HttpMethod.Post;
            request.Content = new FormUrlEncodedContent(new Dictionary<string, string>());

            var validator = new BearerTokenUsageValidator();
            var result = await validator.ValidateAsync(request);

            Assert.IsFalse(result.TokenFound);
        }

        [TestMethod]
        [TestCategory(Category)]
        public async Task Non_Bearer_Scheme_Header()
        {
            var request = new HttpRequestMessage();
            request.Method = HttpMethod.Get;
            request.Headers.Add("Authorization", "Foo Bar");

            var validator = new BearerTokenUsageValidator();
            var result = await validator.ValidateAsync(request);

            Assert.IsFalse(result.TokenFound);
        }

        [TestMethod]
        [TestCategory(Category)]
        public async Task Empty_Bearer_Scheme_Header()
        {
            var request = new HttpRequestMessage();
            request.Method = HttpMethod.Get;
            request.Headers.Add("Authorization", "Bearer");

            var validator = new BearerTokenUsageValidator();
            var result = await validator.ValidateAsync(request);

            Assert.IsFalse(result.TokenFound);
        }

        [TestMethod]
        [TestCategory(Category)]
        public async Task Whitespaces_Bearer_Scheme_Header()
        {
            var request = new HttpRequestMessage();
            request.Method = HttpMethod.Get;
            request.Headers.Add("Authorization", "Bearer           ");

            var validator = new BearerTokenUsageValidator();
            var result = await validator.ValidateAsync(request);

            Assert.IsFalse(result.TokenFound);
        }

        [TestMethod]
        [TestCategory(Category)]
        public async Task Valid_Bearer_Scheme_Header()
        {
            var request = new HttpRequestMessage();
            request.Method = HttpMethod.Get;
            request.Headers.Add("Authorization", "Bearer token");

            var validator = new BearerTokenUsageValidator();
            var result = await validator.ValidateAsync(request);

            Assert.IsTrue(result.TokenFound);
            Assert.AreEqual("token", result.Token);
            Assert.AreEqual(BearerTokenUsageType.AuthorizationHeader, result.UsageType);
        }

        [TestMethod]
        [TestCategory(Category)]
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

            Assert.IsTrue(result.TokenFound);
            Assert.AreEqual("token", result.Token);
            Assert.AreEqual(BearerTokenUsageType.PostBody, result.UsageType);
        }

        [TestMethod]
        [TestCategory(Category)]
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

            Assert.IsFalse(result.TokenFound);
        }

        [TestMethod]
        [TestCategory(Category)]
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

            Assert.IsFalse(result.TokenFound);
        }

        [TestMethod]
        [TestCategory(Category)]
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

            Assert.IsFalse(result.TokenFound);
        }
    }
}