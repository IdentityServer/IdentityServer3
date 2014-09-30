/*
 * Copyright 2014 Dominick Baier, Brock Allen
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *   http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

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
//        IClientStore _clients = Factory.CreateClientStore();

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