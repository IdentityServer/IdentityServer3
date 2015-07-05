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

using FluentAssertions;
using IdentityServer3.Core;
using IdentityServer3.Core.Validation;
using Microsoft.Owin;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace IdentityServer3.Tests.Validation.Secret_Validation
{
    public class X509CertificateSecretParsing
    {
        const string Category = "Secrets - X.509 Certificate Secret Parsing";

        [Fact]
        public async Task CertificateAndClientIdMissing()
        {
            var parser = new X509CertificateSecretParser();
            var context = new OwinContext();
            context.Request.Body = new MemoryStream();

            var secret = await parser.ParseAsync(context.Environment);

            secret.Should().BeNull();
        }

        [Fact]
        public async Task CertificateMissing()
        {
            var parser = new X509CertificateSecretParser();
            var context = new OwinContext();
            
            var body = "client_id=client";
            context.Request.Body = new MemoryStream(Encoding.UTF8.GetBytes(body));

            var secret = await parser.ParseAsync(context.Environment);

            secret.Should().BeNull();
        }

        [Fact]
        public async Task CertificateAndClientIdPresent()
        {
            var parser = new X509CertificateSecretParser();
            var context = new OwinContext();

            var body = "client_id=client";
            context.Request.Body = new MemoryStream(Encoding.UTF8.GetBytes(body));

            var cert = TestCert.Load();
            context.Set("ssl.ClientCertificate", cert);

            var secret = await parser.ParseAsync(context.Environment);

            secret.Type.Should().Be(Constants.ParsedSecretTypes.X509Certificate);
            secret.Id.Should().Be("client");
            secret.Credential.Should().NotBeNull();
            secret.Credential.ShouldBeEquivalentTo(cert);
        }
    }
}