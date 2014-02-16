using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Specialized;
using Thinktecture.IdentityServer.Core;
using Thinktecture.IdentityServer.Core.Connect;
using Thinktecture.IdentityServer.Core.Services;
using UnitTests.Plumbing;

namespace UnitTests.AuthorizeRequest_Validation
{
    [TestClass]
    public class Authorize_ClientValidation_Token
    {
        ILogger _logger = new DebugLogger();
        ICoreSettings _settings = new TestSettings();

        [TestMethod]
        [TestCategory("Client Validation - Token ResponseType")]
        public void Mixed_Token_Request_Without_OpenId_Scope()
        {
            var parameters = new NameValueCollection();
            parameters.Add(Constants.AuthorizeRequest.ClientId, "implicitclient");
            parameters.Add(Constants.AuthorizeRequest.Scope, "resource profile");
            parameters.Add(Constants.AuthorizeRequest.RedirectUri, "oob://implicit/cb");
            parameters.Add(Constants.AuthorizeRequest.ResponseType, Constants.ResponseTypes.Token);

            var validator = new AuthorizeRequestValidator(_settings, _logger);
            var protocolResult = validator.ValidateProtocol(parameters);
            Assert.AreEqual(false, protocolResult.IsError);

            var clientResult = validator.ValidateClient();
            Assert.IsTrue(clientResult.IsError);
            Assert.AreEqual(ErrorTypes.Client, clientResult.ErrorType);
            Assert.AreEqual(Constants.AuthorizeErrors.InvalidScope, clientResult.Error);
        }
    }
}
