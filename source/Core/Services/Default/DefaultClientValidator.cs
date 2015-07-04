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

//using IdentityServer3.Core.Logging;
//using IdentityServer3.Core.Validation;
//using System.Collections.Generic;
//using System.Threading.Tasks;

//namespace IdentityServer3.Core.Services.Default
//{
//    /// <summary>
//    /// Default client validator implementation (supports basic authentication and post body values using hashed shared secrets and x.509 client certificates with thumbprint validation).
//    /// </summary>
//    public class DefaultClientValidator : IClientValidator
//    {
//        private static readonly ILog Logger = LogProvider.GetCurrentClassLogger();

//        private List<IClientValidator> Validators { get; set; }

//        /// <summary>
//        /// Initializes a new instance of the <see cref="DefaultClientValidator"/> class.
//        /// </summary>
//        /// <param name="clients">The clients.</param>
//        public DefaultClientValidator(IClientStore clients)
//        {
//            var hashedSharedSecretValidator = new HashedClientSecretValidator();
//            var x509thumbprintValidator = new X509CertificateThumbprintClientSecretValidator();

//            var basicAuth = new BasicAuthenticationClientValidator(hashedSharedSecretValidator, clients);
//            var postBody = new PostBodyClientValidator(hashedSharedSecretValidator, clients);
//            var x509 = new X509CertificateClientValidator(x509thumbprintValidator, clients);

//            Validators = new List<IClientValidator>
//            {
//                basicAuth, 
//                postBody,
//                x509
//            };
//        }

//        /// <summary>
//        /// Parses the incoming HTTP request and turns some client credential into a client model
//        /// </summary>
//        /// <param name="environment">The environment.</param>
//        /// <returns>
//        /// A validation result
//        /// </returns>
//        public async Task<ClientSecretValidationResult> ValidateAsync(IDictionary<string, object> environment)
//        {
//            Logger.Info("Starting client validation.");

//            foreach (var val in Validators)
//            {
//                var result = await val.ValidateAsync(environment);

//                if (result.IsError)
//                {
//                    return result;
//                }

//                if (result.IsError == false && result.Client != null)
//                {
//                    return result;
//                }
//            }

//            Logger.Info("No client credentials found.");

//            return new ClientSecretValidationResult
//            {
//                IsError = true,
//                Error = "No client credentials found."
//            };
//        }
//    }
//}