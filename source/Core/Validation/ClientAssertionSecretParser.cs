/*
 * Copyright 2014, 2015 Dominick Baier, Brock Allen
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

using IdentityServer3.Core.Extensions;
using IdentityServer3.Core.Logging;
using IdentityServer3.Core.Models;
using IdentityServer3.Core.Services;
using Microsoft.Owin;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens;
using System.Threading.Tasks;

namespace IdentityServer3.Core.Validation
{
    /// <summary>
    /// Parses a POST body for secrets
    /// </summary>
    public class ClientAssertionSecretParser : ISecretParser
    {
        private static readonly ILog Logger = LogProvider.GetCurrentClassLogger();

        /// <summary>
        /// Tries to find a JWT client assertion token on the environment that can be used for authentication
        /// Used for "private_key_jwt" client authentication method as defined in http://openid.net/specs/openid-connect-core-1_0.html#ClientAuthentication
        /// </summary>
        /// <param name="environment">The environment.</param>
        /// <returns>
        /// A parsed secret
        /// </returns>
        public async Task<ParsedSecret> ParseAsync(IDictionary<string, object> environment)
        {
            Logger.Debug("Start parsing for JWT client assertion in post body");

            var context = new OwinContext(environment);
            var body = await context.ReadRequestFormAsync();

            if (body != null)
            {
                var clientId = body.Get(Constants.TokenRequest.ClientId);
                var clientAssertionType = body.Get(Constants.TokenRequest.ClientAssertionType);
                var clientAssertion = body.Get(Constants.TokenRequest.ClientAssertion);

                if (clientAssertion.IsPresent()
                    && clientAssertionType == Constants.ClientAssertionTypes.JwtBearer)
                {
                    if (!clientId.IsPresent())
                    {
                        // at least some clients (i.e. java com.nimbusds/oauth2-oidc-sdk) do not send client_id, but assume that token is enough (and it actually is)
                        clientId = GetClientIdFromToken(clientAssertion);
                        if (!clientId.IsPresent())
                        {
                            return null;
                        }
                    }
                    var parsedSecret = new ParsedSecret
                    {
                        Id = clientId,
                        Credential = clientAssertion,
                        Type = Constants.ParsedSecretTypes.JwtBearer
                    };

                    return parsedSecret;
                }
            }

            Logger.Debug("No JWT client assertion found in post body");
            return null;
        }

        private string GetClientIdFromToken(string token)
        {
            try
            {
                var jwt = new JwtSecurityToken(token);
                return jwt.Issuer;
            }
            catch (Exception e)
            {
                Logger.WarnException("Could not parse client assertion", e);
                return null;
            }
        }
    }
}
