using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Specialized;
using Thinktecture.IdentityServer.Core;
using Thinktecture.IdentityServer.Core.Connect;
using Thinktecture.IdentityServer.Core.Services;
using UnitTests.Plumbing;

namespace UnitTests
{
    [TestClass]
    public class Authorize_ClientValidation_IdToken
    {
        ILogger _logger = new DebugLogger();
        ICoreSettings _settings = new TestSettings();

        [TestMethod]
        [TestCategory("Client Validation - IdToken ResponseType")]
        public void Mixed_IdToken_Request()
        {
            var parameters = new NameValueCollection();
            parameters.Add(Constants.AuthorizeRequest.ClientId, "implicitclient");
            parameters.Add(Constants.AuthorizeRequest.Scope, "openid resource");
            parameters.Add(Constants.AuthorizeRequest.RedirectUri, "oob://implicit/cb");
            parameters.Add(Constants.AuthorizeRequest.ResponseType, Constants.ResponseTypes.IdToken);
            parameters.Add(Constants.AuthorizeRequest.Nonce, "abc");

            var validator = new AuthorizeRequestValidator(_settings, _logger);
            var protocolResult = validator.ValidateProtocol(parameters);
            Assert.AreEqual(false, protocolResult.IsError);

            var clientResult = validator.ValidateClient();
            Assert.AreEqual(true, clientResult.IsError);
            Assert.AreEqual(ErrorTypes.Client, clientResult.ErrorType);
            Assert.AreEqual(Constants.AuthorizeErrors.InvalidScope, clientResult.Error);
        }
    }
}