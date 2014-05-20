/*
 * Copyright (c) Dominick Baier, Brock Allen.  All rights reserved.
 * see license
 */
using Microsoft.Owin;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Thinktecture.IdentityServer.WsFed.Configuration;

namespace Thinktecture.IdentityServer.WsFed.Services
{
    public class CookieMiddlewareCookieService : ICookieService
    {
        private IOwinContext _context;

        public CookieMiddlewareCookieService(IOwinContext context)
        {
            _context = context;
        }

        public async Task AddValueAsync(string value)
        {
            var urls = await GetValuesAsync();

            var duplicateUrl = urls.FirstOrDefault(s => s == value);
            if (duplicateUrl != null)
            {
                return;
            }

            urls.Add(value);

            var claims = new List<Claim>(from u in urls select new Claim("url", u));
            var id = new ClaimsIdentity(claims, WsFederationPluginOptions.WsFedCookieAuthenticationType);

            _context.Authentication.SignIn(id);
        }

        public async Task<IEnumerable<string>> GetValuesAndDeleteCookieAsync()
        {
            var urls = await GetValuesAsync();
            _context.Authentication.SignOut(WsFederationPluginOptions.WsFedCookieAuthenticationType);

            return urls;
        }

        async Task<List<string>> GetValuesAsync()
        {
            var result = await _context.Authentication.AuthenticateAsync(WsFederationPluginOptions.WsFedCookieAuthenticationType);
            
            if (result == null || result.Identity == null)
            {
                return new List<string>();
            }

            var urls = from c in result.Identity.Claims
                       where c.Type == "url"
                       select c.Value;

            return urls.ToList();
        }
    }
}