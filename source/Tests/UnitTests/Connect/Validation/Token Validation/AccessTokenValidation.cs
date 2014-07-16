//using Microsoft.VisualStudio.TestTools.UnitTesting;
//using System.Threading.Tasks;
//using Thinktecture.IdentityServer.Core;
//using Thinktecture.IdentityServer.Core.Connect;
//using Thinktecture.IdentityServer.Core.Connect.Models;
//using Thinktecture.IdentityServer.Core.Connect.Services;
//using Thinktecture.IdentityServer.Core.Services;
//using UnitTests.Plumbing;

//namespace Thinktecture.IdentityServer.Tests.Validation_Tests.Token_Validation
//{
//    [TestClass]
//    public class AccessTokenValidation
//    {
//        TestSettings _settings = new TestSettings();
//        IClientService _clients = Factory.CreateClientService();

//        [TestMethod]
//        public async Task Create_and_Validate_JWT_AccessToken_Valid()
//        {
//            var tokenService = new DefaultTokenService(
//                null,
//                _settings,
//                null,
//                null);

//            var token = new Token(Constants.TokenTypes.AccessToken)
//            {
//                Audience = string.Format(Constants.AccessTokenAudience, _settings.IssuerUri),
//                Issuer = _settings.IssuerUri,
//                Lifetime = 60,
//                Client = await _clients.FindClientByIdAsync("client")
//            };

//            var jwt = await tokenService.CreateSecurityTokenAsync(token);

//            var validator = new TokenValidator(_settings, null, null);
//            var result = await validator.ValidateAccessTokenAsync(jwt);

//            Assert.IsFalse(result.IsError);
//            Assert.IsNotNull(result.Claims);
//        }
//    }
//}