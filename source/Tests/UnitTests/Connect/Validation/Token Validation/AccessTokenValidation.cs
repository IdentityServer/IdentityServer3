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

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading.Tasks;
using Thinktecture.IdentityServer.Core;
using Thinktecture.IdentityServer.Core.Connect;
using Thinktecture.IdentityServer.Core.Connect.Models;
using Thinktecture.IdentityServer.Core.Services;
using Thinktecture.IdentityServer.Core.Services.InMemory;
using Thinktecture.IdentityServer.Tests.Connect.Setup;

namespace Thinktecture.IdentityServer.Tests.Connect.Validation.Tokens
{
    [TestClass]
    public class AccessTokenValidation
    {
        const string Category = "Access token validation";

        IClientStore _clients = Factory.CreateClientStore();
        
        [TestMethod]
        [TestCategory(Category)]
        public void Valid_Reference_Token()
        {
            throw new NotImplementedException();
        }

        [TestMethod]
        [TestCategory(Category)]
        public void Valid_JWT_Token()
        {
            throw new NotImplementedException();
        }

        [TestMethod]
        [TestCategory(Category)]
        public void Malformed_JWT_Token()
        {
            throw new NotImplementedException();
        }


        [TestMethod]
        [TestCategory(Category)]
        public void Expired_JWT_Token()
        {
            throw new NotImplementedException();
        }

        [TestMethod]
        [TestCategory(Category)]
        public void Valid_JWT_Token_wrong_Issuer()
        {
            throw new NotImplementedException();
        }


        [TestMethod]
        [TestCategory(Category)]
        public void Valid_JWT_Token_missing_Scope()
        {
            throw new NotImplementedException();
        }

        [TestMethod]
        [TestCategory(Category)]
        public void Valid_JWT_Token_but_User_not_active()
        {
            throw new NotImplementedException();
        }

        [TestMethod]
        [TestCategory(Category)]
        public void Valid_JWT_Token_but_Client_not_active()
        {
            throw new NotImplementedException();
        }

        [TestMethod]
        [TestCategory(Category)]
        public void Unknown_Reference_Token()
        {
            throw new NotImplementedException();
        }

        [TestMethod]
        [TestCategory(Category)]
        public void Expired_Reference_Token()
        {
            throw new NotImplementedException();
        }

        [TestMethod]
        [TestCategory(Category)]
        public void Valid_Reference_Token_but_User_not_active()
        {
            throw new NotImplementedException();
        }

        [TestMethod]
        [TestCategory(Category)]
        public void Valid_Reference_Token_but_Client_not_active()
        {
            throw new NotImplementedException();
        }

        [TestMethod]
        [TestCategory(Category)]
        public void Valid_Reference_Token_missing_Scope()
        {
            throw new NotImplementedException();
        }
    }
}