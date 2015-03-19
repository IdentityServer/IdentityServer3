///*
// * Copyright 2014, 2015 Dominick Baier, Brock Allen
// *
// * Licensed under the Apache License, Version 2.0 (the "License");
// * you may not use this file except in compliance with the License.
// * You may obtain a copy of the License at
// *
// *   http://www.apache.org/licenses/LICENSE-2.0
// *
// * Unless required by applicable law or agreed to in writing, software
// * distributed under the License is distributed on an "AS IS" BASIS,
// * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// * See the License for the specific language governing permissions and
// * limitations under the License.
// */

//using System;
//using System.Threading.Tasks;
//using Thinktecture.IdentityServer.Core.Models;

//namespace Thinktecture.IdentityServer.Core.Services.Default
//{
//    /// <summary>
//    /// Default client secret validator can deal with hashed shared secrets or client certificates using thumbprints
//    /// </summary>
//    public class DefaultClientSecretValidator : IClientSecretValidator
//    {
//        /// <summary>
//        /// Validates the client secret.
//        /// </summary>
//        /// <param name="client">The client.</param>
//        /// <param name="credential">The client credential.</param>
//        /// <returns></returns>
//        /// <exception cref="System.InvalidOperationException">Invalid client authentication method</exception>
//        public Task<bool> ValidateClientSecretAsync(Client client, ClientCredential credential)
//        {
//            if (credential.AuthenticationMethod == ClientAuthenticationMethods.Basic ||
//                credential.AuthenticationMethod == ClientAuthenticationMethods.FormPost)
//            {
//                return new HashedClientSecretValidator().ValidateClientSecretAsync(client, credential);
//            }

//            if (credential.AuthenticationMethod == ClientAuthenticationMethods.X509Certificate)
//            {
//                return new X509CertificateThumbprintClientSecretValidator().ValidateClientSecretAsync(client, credential);
//            }

//            throw new InvalidOperationException("Invalid client authentication method");
//        }
//    }
//}