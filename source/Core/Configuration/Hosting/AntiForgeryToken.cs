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
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Microsoft.Owin;
using Thinktecture.IdentityModel;
using Thinktecture.IdentityServer.Core.App_Packages.LibLog._2._0;
using Thinktecture.IdentityServer.Core.Extensions;
using Thinktecture.IdentityServer.Core.ViewModels;

#pragma warning disable 1591

namespace Thinktecture.IdentityServer.Core.Configuration.Hosting
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class AntiForgeryToken
    {
        private readonly static ILog Logger = LogProvider.GetCurrentClassLogger();

        const string TokenName = "idsrv.xsrf";
        const string CookieEntropy = TokenName + "AntiForgeryTokenCookie";
        const string HiddenInputEntropy = TokenName + "AntiForgeryTokenHidden";

        readonly IOwinContext _context;
        readonly IdentityServerOptions _options;

        internal AntiForgeryToken(IOwinContext context, IdentityServerOptions options)
        {
            _context = context;
            _options = options;
        }

        internal AntiForgeryTokenViewModel GetAntiForgeryToken()
        {
            var tokenBytes = GetCookieToken();
            var protectedTokenBytes = _options.DataProtector.Protect(tokenBytes, HiddenInputEntropy);
            var token = Base64Url.Encode(protectedTokenBytes);

            return new AntiForgeryTokenViewModel
            {
                Name = TokenName,
                Value = token
            };
        }
        
        internal async Task<bool> IsTokenValid()
        {
            try
            {
                var cookieToken = GetCookieToken();
                var hiddenInputToken = await GetHiddenInputTokenAsync();
                return CompareByteArrays(cookieToken, hiddenInputToken);
            }
            catch(Exception ex)
            {
                Logger.ErrorException("AntiForgeryTokenValidator validating token", ex);
            }
            return false;
        }

        [MethodImpl(MethodImplOptions.NoOptimization)]
        bool CompareByteArrays(byte[] cookieToken, byte[] hiddenInputToken)
        {
            if (cookieToken == null || hiddenInputToken == null) return false;
            if (cookieToken.Length != hiddenInputToken.Length) return false;
            
            var same = true;
            for(var i = 0; i < cookieToken.Length; i++)
            {
                same &= (cookieToken[i] == hiddenInputToken[i]);
            }
            return same;
        }

        byte[] GetCookieToken()
        {
            var cookieName = _options.AuthenticationOptions.CookieOptions.Prefix + TokenName;
            var cookie = _context.Request.Cookies[cookieName];

            if (cookie != null)
            {
                try
                {
                    var protectedCookieBytes = Base64Url.Decode(cookie);
                    var tokenBytes = _options.DataProtector.Unprotect(protectedCookieBytes, CookieEntropy);
                    return tokenBytes;
                }
                catch(Exception ex)
                {
                    // if there's an exception we fall thru the catch block to reissue a new cookie
                    Logger.WarnFormat("Problem unprotecting cookie; Issuing new cookie. Error message: {0}", ex.Message);
                }
            }

            var bytes = CryptoRandom.CreateRandomKey(16);
            var protectedTokenBytes = _options.DataProtector.Protect(bytes, CookieEntropy);
            var token = Base64Url.Encode(protectedTokenBytes);
            
            var secure = _context.Request.Scheme == Uri.UriSchemeHttps;
            var path = _context.Request.Environment.GetIdentityServerBasePath().CleanUrlPath();
            _context.Response.Cookies.Append(cookieName, token, new Microsoft.Owin.CookieOptions
            {
                HttpOnly = true,
                Secure = secure,
                Path = path
            });

            return bytes;
        }

        async Task<byte[]> GetHiddenInputTokenAsync()
        {
            // hack to clear a possible cached type from Katana in environment
            _context.Environment.Remove("Microsoft.Owin.Form#collection");

            if (!_context.Request.Body.CanSeek)
            {
                var copy = new MemoryStream();
                await _context.Request.Body.CopyToAsync(copy);
                copy.Seek(0L, SeekOrigin.Begin);
                _context.Request.Body = copy;
            }
            var form = await _context.Request.ReadFormAsync();
            _context.Request.Body.Seek(0L, SeekOrigin.Begin);

            // hack to prevent caching of an internalized type from Katana in environment
            _context.Environment.Remove("Microsoft.Owin.Form#collection");

            var token = form[TokenName];
            if (token == null) return null;

            var tokenBytes = Base64Url.Decode(token);
            var unprotectedTokenBytes = _options.DataProtector.Unprotect(tokenBytes, HiddenInputEntropy);
            
            return unprotectedTokenBytes;
        }
    }
}
