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

using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using IdentityModel;
using IdentityServer3.Core.Configuration;
using IdentityServer3.Core.Extensions;
using IdentityServer3.Core.Logging;
using IdentityServer3.Core.Models;
using IdentityServer3.Core.Services;

namespace IdentityServer3.Core.Validation
{
    /// <summary>
    /// Validates a secret based on RS256 signed JWT token
    /// </summary>
    public class PrivateKeyJwtSecretValidator : ISecretValidator
    {
        private static readonly ILog Logger = LogProvider.GetCurrentClassLogger();
        private readonly IClientAssertionTokenReplayCache tokenReplayCache;
        private readonly string audienceUri;

        /// <summary>
        /// Instantiates an instance of private_key_jwt secret validator
        /// </summary>
        /// <param name="options">IdentityServer options</param>
        /// <param name="tokenReplayCache">Token replay cache</param>
        public PrivateKeyJwtSecretValidator(IdentityServerOptions options, IClientAssertionTokenReplayCache tokenReplayCache)
        {
            audienceUri = string.Concat(options.DynamicallyCalculatedIssuerUri.EnsureTrailingSlash(), Constants.RoutePaths.Oidc.Token);
            this.tokenReplayCache = tokenReplayCache;
        }

        /// <summary>
        /// Validates a secret
        /// </summary>
        /// <param name="secrets">The stored secrets.</param>
        /// <param name="parsedSecret">The received secret.</param>
        /// <returns>
        /// A validation result
        /// </returns>
        /// <exception cref="System.ArgumentException">ParsedSecret.Credential is not a JWT token</exception>
        public Task<SecretValidationResult> ValidateAsync(IEnumerable<Secret> secrets, ParsedSecret parsedSecret)
        {
            var fail = Task.FromResult(new SecretValidationResult { Success = false });
            var success = Task.FromResult(new SecretValidationResult { Success = true });

            if (parsedSecret.Type != Constants.ParsedSecretTypes.JwtBearer)
            {
                return fail;
            }

            var jwtTokenString = parsedSecret.Credential as string;

            if (jwtTokenString == null)
            {
                throw new ArgumentException("ParsedSecret.Credential is not a string.");
            }

            var enumeratedSecrets = secrets as IList<Secret> ?? secrets.ToList();

            var embeddedKey = GetTrustedEmbeddedCertificateKey(enumeratedSecrets, jwtTokenString);

            var savedKeys = (embeddedKey == null)
                ? GetTrustedCertificateKeys(enumeratedSecrets)
                : null;

            if (embeddedKey == null
                && !savedKeys.Any()
                && enumeratedSecrets.Any(s => s.Type == Constants.SecretTypes.X509CertificateThumbprint))
            {
                Logger.Warn("Cannot validate client assertion token that does not embed full certificate using only thumbprint secret");
                return fail;
            }

            var tokenValidationParameters = new TokenValidationParameters
            {
                IssuerSigningKey = embeddedKey,
                IssuerSigningKeys = savedKeys,
                ValidateIssuerSigningKey = true,

                ValidIssuer = parsedSecret.Id,
                ValidateIssuer = true,

                ValidAudience = audienceUri,
                ValidateAudience = true,

                RequireSignedTokens = true,
                RequireExpirationTime = true,

                TokenReplayCache = new TokenReplayCacheWrapper(tokenReplayCache)
            };
            try
            {
                SecurityToken token;
                var handler = new EmbeddedCertificateJwtSecurityTokenHandler();
                handler.ValidateToken(jwtTokenString, tokenValidationParameters, out token);

                var jwtToken = (JwtSecurityToken) token;

                if (jwtToken.Subject != jwtToken.Issuer)
                {
                    return fail;
                }

                return success;
            }
            catch (Exception e)
            {
                Logger.Debug("JWT token validation error: " + e.Message);
                return fail;
            }
        }

        private static SecurityKey GetTrustedEmbeddedCertificateKey(IEnumerable<Secret> secrets, string jwtTokenString)
        {
            SecurityKey securityKey = null;
            var certificate = new JwtSecurityToken(jwtTokenString).GetCertificateFromToken();
            if (certificate != null
                && certificate.Thumbprint != null
                && secrets.Any(s => !s.Expiration.HasExpired()
                                    && TimeConstantComparer.IsEqual(s.Value.ToLowerInvariant(), certificate.Thumbprint.ToLowerInvariant())))
            {
                securityKey = new X509SecurityKey(certificate);
            }
            return securityKey;
        }

        private List<SecurityKey> GetTrustedCertificateKeys(IEnumerable<Secret> secrets)
        {
            var securityKeys = secrets
                .Where(s => !s.Expiration.HasExpired()
                            && s.Type == Constants.SecretTypes.X509CertificateBase64)
                .Select(s => GetCertificateFromString(s.Value))
                .Where(c => c != null)
                .Select(c => new X509SecurityKey(c))
                .Cast<SecurityKey>()
                .ToList();
            return securityKeys;
        }

        private static X509Certificate2 GetCertificateFromString(string value)
        {
            try
            {
                return new X509Certificate2(Convert.FromBase64String(value));
            }
            catch
            {
                Logger.Warn("Could not read certificate from string: " + value);
                return null;
            }
        }
    }
}
