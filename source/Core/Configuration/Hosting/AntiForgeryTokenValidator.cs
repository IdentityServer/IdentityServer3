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
using Microsoft.Owin;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Thinktecture.IdentityModel;
using Thinktecture.IdentityServer.Core.Extensions;
using Thinktecture.IdentityServer.Core.Logging;
using Thinktecture.IdentityServer.Core.ViewModels;

namespace Thinktecture.IdentityServer.Core.Configuration.Hosting
{
    internal class AntiForgeryTokenValidator
    {
        private readonly static ILog Logger = LogProvider.GetCurrentClassLogger();

        const string TokenName = "idsrv.xsrf";
        const string CookieEntropy = TokenName + "Cookie";
        const string HiddenInputEntropy = TokenName + "Hidden";

        internal static async Task<bool> IsTokenValid(IDictionary<string, object> env)
        {
            try
            {
                var cookieToken = GetCookieToken(env);
                var hiddenInputToken = await GetHiddenInputTokenAsync(env);
                return CompareByteArrays(cookieToken, hiddenInputToken);
            }
            catch(Exception ex)
            {
                Logger.ErrorException("AntiForgeryTokenValidator validating token", ex);
            }
            return false;
        }

        private static bool CompareByteArrays(byte[] cookieToken, byte[] hiddenInputToken)
        {
            if (cookieToken == null || hiddenInputToken == null) return false;
            if (cookieToken.Length != hiddenInputToken.Length) return false;
            for(var i = 0; i < cookieToken.Length; i++)
            {
                if (cookieToken[i] != hiddenInputToken[i]) return false;
            }
            return true;
        }

        internal static byte[] GetCookieToken(IDictionary<string, object> env)
        {
            var options = env.ResolveDependency<IdentityServerOptions>();

            var ctx = new OwinContext(env);
            var cookieName = options.AuthenticationOptions.CookieOptions.Prefix + TokenName;
            var cookie = ctx.Request.Cookies[cookieName];

            if (cookie != null)
            {
                try
                {
                    var protectedCookieBytes = Base64Url.Decode(cookie);
                    var tokenBytes = options.DataProtector.Unprotect(protectedCookieBytes, CookieEntropy);
                    return tokenBytes;
                }
                catch(Exception ex)
                {
                    // if there's an exception we fall thru the catch block to reissue a new cookie
                    Logger.WarnFormat("Problem unprotecting cookie; Issuing new cookie. Error message: {0}", ex.Message);
                }
            }

            var bytes = CryptoRandom.CreateRandomKey(16);
            var protectedTokenBytes = options.DataProtector.Protect(bytes, CookieEntropy);
            var token = Base64Url.Encode(protectedTokenBytes);
            
            var secure = ctx.Request.Scheme == Uri.UriSchemeHttps;
            var path = ctx.Request.Environment.GetIdentityServerBasePath();
            if (path.EndsWith("/")) path = path.Substring(0, path.Length - 1);
            if (String.IsNullOrWhiteSpace(path)) path = "/";
            ctx.Response.Cookies.Append(cookieName, token, new Microsoft.Owin.CookieOptions
            {
                HttpOnly = true,
                Secure = secure,
                Path = path
            });

            return bytes;
        }

        internal static async Task<byte[]> GetHiddenInputTokenAsync(IDictionary<string, object> env)
        {
            var ctx = new OwinContext(env);

            // hack to clear a possible cached type from Katana in environment
            ctx.Environment.Remove("Microsoft.Owin.Form#collection");

            if (!ctx.Request.Body.CanSeek)
            {
                var copy = new MemoryStream();
                await ctx.Request.Body.CopyToAsync(copy);
                copy.Seek(0L, SeekOrigin.Begin);
                ctx.Request.Body = copy;
            }
            var form = await ctx.Request.ReadFormAsync();
            ctx.Request.Body.Seek(0L, SeekOrigin.Begin);

            // hack to prevent caching of an internalized type from Katana in environment
            ctx.Environment.Remove("Microsoft.Owin.Form#collection");

            var token = form[TokenName];
            if (token == null) return null;
            var tokenBytes = Base64Url.Decode(token);

            var options = env.ResolveDependency<IdentityServerOptions>();
            var unprotectedTokenBytes = options.DataProtector.Unprotect(tokenBytes, HiddenInputEntropy);
            return unprotectedTokenBytes;
        }

        internal static AntiForgeryHiddenInputViewModel GetAntiForgeryHiddenInput(IDictionary<string, object> env)
        {
            var options = env.ResolveDependency<IdentityServerOptions>();
            
            var tokenBytes = GetCookieToken(env);
            var protectedTokenBytes = options.DataProtector.Protect(tokenBytes, HiddenInputEntropy);
            var token = Base64Url.Encode(protectedTokenBytes);

            return new AntiForgeryHiddenInputViewModel
            {
                Name = TokenName,
                Value = token
            };
        }
    }
}
