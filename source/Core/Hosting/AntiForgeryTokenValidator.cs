using Microsoft.Owin;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Thinktecture.IdentityServer.Core.Views;
using Thinktecture.IdentityServer.Core.Extensions;
using Thinktecture.IdentityServer.Core.Configuration;

namespace Thinktecture.IdentityServer.Core.Hosting
{
    class AntiForgeryTokenValidator
    {
        const string TokenName = "idsrv.xsrf";
        const string CookieEntropy = TokenName + "Cookie";
        const string HiddenInputEntropy = TokenName + "Hidden";

        internal static async Task<bool> IsTokenValid(IDictionary<string, object> env)
        {
            var cookieToken = GetCookieToken(env);
            var hiddenInputToken = await GetHiddenInputTokenAsync(env);

            var g1 = new Guid(cookieToken);
            var g2 = new Guid(hiddenInputToken);
            return g1.Equals(g2);
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
                var token = Thinktecture.IdentityModel.Base64Url.Encode(protectedTokenBytes);
                ctx.Response.Cookies.Append(TokenName, token);

                return bytes;
            }

            var protectedCookieBytes = Thinktecture.IdentityModel.Base64Url.Decode(cookie);
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
            var tokenBytes = Thinktecture.IdentityModel.Base64Url.Decode(token);

            var options = env.ResolveDependency<IdentityServerOptions>();
            var unprotectedTokenBytes = options.DataProtector.Unprotect(tokenBytes, HiddenInputEntropy);
            return unprotectedTokenBytes;
        }

        internal static AntiForgeryHiddenInputViewModel GetAntiForgeryHiddenInput(IDictionary<string, object> env)
        {
            var options = env.ResolveDependency<IdentityServerOptions>();
            
            var tokenBytes = GetCookieToken(env);
            var protectedTokenBytes = options.DataProtector.Protect(tokenBytes, HiddenInputEntropy);
            var token = Thinktecture.IdentityModel.Base64Url.Encode(protectedTokenBytes);

            return new AntiForgeryHiddenInputViewModel
            {
                Name = TokenName,
                Value = token
            };
        }
    }
}
