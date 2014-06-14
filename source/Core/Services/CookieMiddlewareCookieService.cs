/*
 * Copyright (c) Dominick Baier, Brock Allen.  All rights reserved.
 * see license
 */
using Microsoft.Owin;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Thinktecture.IdentityServer.Core.Logging;

namespace Thinktecture.IdentityServer.Core.Services
{
    public class CookieMiddlewareTrackingCookieService : ITrackingCookieService
    {
        private readonly IOwinContext _context;
        private readonly ILog _logger;

        public CookieMiddlewareTrackingCookieService(IOwinContext context)
        {
            _context = context;
            _logger = LogProvider.GetCurrentClassLogger();
        }

        public async Task AddValueAsync(string name, string value)
        {
            var urls = await GetValuesAsync(name);

            var duplicateUrl = urls.FirstOrDefault(s => s == value);
            if (duplicateUrl != null)
            {
                _logger.DebugFormat("{0} already exists in {1} cookie", value, name);
                return;
            }

            _logger.DebugFormat("Adding {0} to {1} cookie", value, name);
            urls.Add(value);

            var claims = new List<Claim>(from u in urls select new Claim("url", u));
            var id = new ClaimsIdentity(claims, name);

            _context.Authentication.SignIn(id);
        }

        public async Task<IEnumerable<string>> GetValuesAndDeleteCookieAsync(string name)
        {
            var urls = await GetValuesAsync(name);

            _logger.DebugFormat("Removing cookie {0}", name);
            _context.Authentication.SignOut(name);

            return urls;
        }

        async Task<List<string>> GetValuesAsync(string name)
        {
            _logger.DebugFormat("Retrieving values of cookie {0}", name);
            var result = await _context.Authentication.AuthenticateAsync(name);
            
            if (result == null || result.Identity == null)
            {
                _logger.DebugFormat("Cookie {0} does not exist", name);
                return new List<string>();
            }

            var urls = from c in result.Identity.Claims
                       where c.Type == "url"
                       select c.Value;

            return urls.ToList();
        }
    }
}