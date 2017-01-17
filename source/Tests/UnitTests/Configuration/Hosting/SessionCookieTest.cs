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

using System.Collections.Generic;
using FluentAssertions;
using IdentityServer3.Core;
using IdentityServer3.Core.Configuration;
using IdentityServer3.Core.Configuration.Hosting;
using IdentityServer3.Core.Extensions;
using Microsoft.Owin;
using Xunit;

namespace IdentityServer3.Tests.Configuration.Hosting
{
    public class SessionCookieTest
    {
        private static IdentityServerOptions ServerOptions()
        {
            return new IdentityServerOptions();
        }

        private static string GetCookieName()
        {
            var identityServerOptions = ServerOptions();
            return identityServerOptions.AuthenticationOptions.CookieOptions.GetSessionCookieName();
        }

        private static OwinContext Context(Dictionary<string, string[]> responseHeaders, string sid = null)
        {
            var env = new Dictionary<string, object>
            {
                {Constants.OwinEnvironment.IdentityServerBasePath, ""}
            };
            env.SetIdentityServerHost("https://identityserver.io");

            var headers = new Dictionary<string, string[]> {{"Host", new[] {"identityserver.io"}}};
            env.Add("owin.RequestHeaders", headers);

            env.Add("owin.ResponseHeaders", responseHeaders);

            if (sid != null)
            {
                var cookies = new Dictionary<string, string>
                {
                    {GetCookieName(), sid}
                };
                env.Add("Microsoft.Owin.Cookies#dictionary", cookies);
            }

            return new OwinContext(env);
        }

        [Fact]
        public void IssueSessionId_Create_SessionCookie_When_Not_Already_Exist()
        {
            var identityServerOptions = ServerOptions();
            var responseHeaders = new Dictionary<string, string[]>();
            var context = Context(responseHeaders);
            var sessionCookie = new SessionCookie(context, identityServerOptions);

            context.Request.Cookies[GetCookieName()].Should().BeNull();
            sessionCookie.IssueSessionId(false);
            responseHeaders.ContainsKey("Set-Cookie").Should().BeTrue();
            responseHeaders["Set-Cookie"].Length.Should().Be(1);
            responseHeaders["Set-Cookie"][0].Should().Contain(GetCookieName());
        }

        [Fact]
        public void IssueSessionId_Not_Create_SessionCookieName_When_Already_Exist()
        {
            var identityServerOptions = ServerOptions();
            var responseHeaders = new Dictionary<string, string[]>();
            var context = Context(responseHeaders, "46259aebd700e600d743967df02997e6");
            var sessionCookie = new SessionCookie(context, identityServerOptions);

            context.Request.Cookies[GetCookieName()].Should().NotBeNullOrWhiteSpace();
            sessionCookie.IssueSessionId(false);
            responseHeaders.ContainsKey("Set-Cookie").Should().BeFalse();
        }
    }
}