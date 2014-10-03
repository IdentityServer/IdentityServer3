using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IdentityModel.Tokens;
using System.Threading.Tasks;
using Thinktecture.IdentityModel.Tokens;
using Thinktecture.IdentityServer.Core;
using Thinktecture.IdentityServer.Core.Services;
using Thinktecture.IdentityServer.Tests.Connect.Setup;

namespace Thinktecture.IdentityServer.Tests.Connect.Validation.Tokens
{
    [TestClass]
    public class IdentityTokenValidation
    {
        const string Category = "Identity token validation";

        static IdentityTokenValidation()
        {
            JwtSecurityTokenHandler.InboundClaimTypeMap = ClaimMappings.None;
        }

        [TestMethod]
        [TestCategory(Category)]
        public async Task Valid_IdentityToken_DefaultKeyType()
        {
            var signer = new DefaultTokenSigningService(TestIdentityServerOptions.Create());
            var jwt = await signer.SignTokenAsync(TokenFactory.CreateIdentityToken("roclient", "valid"));
            var validator = Factory.CreateTokenValidator();

            var result = await validator.ValidateIdentityTokenAsync(jwt, "roclient");
            Assert.IsFalse(result.IsError);
        }

        [TestMethod]
        [TestCategory(Category)]
        public async Task Valid_IdentityToken_DefaultKeyType_no_ClientId_supplied()
        {
            var signer = new DefaultTokenSigningService(TestIdentityServerOptions.Create());
            var jwt = await signer.SignTokenAsync(TokenFactory.CreateIdentityToken("roclient", "valid"));
            var validator = Factory.CreateTokenValidator();

            var result = await validator.ValidateIdentityTokenAsync(jwt, "roclient");
            Assert.IsFalse(result.IsError);
        }

        [TestMethod]
        [TestCategory(Category)]
        public async Task Valid_IdentityToken_no_ClientId_supplied()
        {
            var signer = new DefaultTokenSigningService(TestIdentityServerOptions.Create());
            var jwt = await signer.SignTokenAsync(TokenFactory.CreateIdentityToken("roclient", "valid"));
            var validator = Factory.CreateTokenValidator();

            var result = await validator.ValidateIdentityTokenAsync(jwt);
            Assert.IsFalse(result.IsError);
        }

        [TestMethod]
        [TestCategory(Category)]
        public async Task IdentityToken_InvalidClientId()
        {
            var signer = new DefaultTokenSigningService(TestIdentityServerOptions.Create());
            var jwt = await signer.SignTokenAsync(TokenFactory.CreateIdentityToken("roclient", "valid"));
            var validator = Factory.CreateTokenValidator();

            var result = await validator.ValidateIdentityTokenAsync(jwt, "invalid");
            Assert.IsTrue(result.IsError);
            Assert.AreEqual(Constants.ProtectedResourceErrors.InvalidToken, result.Error);
        }

        

        [TestMethod]
        [TestCategory(Category)]
        public async Task Valid_IdentityToken_SymmetricKeyType()
        {
            var signer = new DefaultTokenSigningService(TestIdentityServerOptions.Create());
            var jwt = await signer.SignTokenAsync(TokenFactory.CreateIdentityToken("roclient_symmetric", "valid"));
            var validator = Factory.CreateTokenValidator();

            var result = await validator.ValidateIdentityTokenAsync(jwt, "roclient_symmetric");
            Assert.IsFalse(result.IsError);
        }

        [TestCategory(Category)]
        public async Task Valid_IdentityToken_SymmetricKeyType_no_ClientId_supplied()
        {
            var signer = new DefaultTokenSigningService(TestIdentityServerOptions.Create());
            var jwt = await signer.SignTokenAsync(TokenFactory.CreateIdentityToken("roclient_symmetric", "valid"));
            var validator = Factory.CreateTokenValidator();

            var result = await validator.ValidateIdentityTokenAsync(jwt);
            Assert.IsFalse(result.IsError);
        }
    }
}