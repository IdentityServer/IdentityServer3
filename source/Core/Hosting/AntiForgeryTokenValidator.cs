using Microsoft.Owin;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Thinktecture.IdentityServer.Core.Views;
using Thinktecture.IdentityServer.Core.Extensions;
using Thinktecture.IdentityServer.Core.Configuration;
using Thinktecture.IdentityServer.Core.Logging;
using Thinktecture.IdentityModel;

namespace Thinktecture.IdentityServer.Core.Hosting
{
    class AntiForgeryTokenValidator
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
            var cookie = ctx.Request.Cookies[TokenName];

            if (cookie == null)
            {
                var bytes = Guid.NewGuid().ToByteArray();

                var protectedTokenBytes = options.DataProtector.Protect(bytes, CookieEntropy);
                var token = Base64Url.Encode(protectedTokenBytes);
                ctx.Response.Cookies.Append(TokenName, token);

                return bytes;
            }

            var protectedCookieBytes = Base64Url.Decode(cookie);
            var tokenBytes = options.DataProtector.Unprotect(protectedCookieBytes, CookieEntropy);
            return tokenBytes;
        }

        internal static async Task<byte[]> GetHiddenInputTokenAsync(IDictionary<string, object> env)
        {
            var ctx = new OwinContext(env);
            if (!ctx.Request.Body.CanSeek)
            {
                var copy = new MemoryStream();
                await ctx.Request.Body.CopyToAsync(copy);
                copy.Seek(0L, SeekOrigin.Begin);
                ctx.Request.Body = copy;
            }
            var form = await ctx.Request.ReadFormAsync();
            ctx.Request.Body.Seek(0L, SeekOrigin.Begin);

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
