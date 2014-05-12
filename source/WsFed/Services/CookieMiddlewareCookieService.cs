//using Microsoft.Owin;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using Thinktecture.IdentityServer.WsFed.Configuration;

//namespace Thinktecture.IdentityServer.WsFed.Services
//{
//    public class CookieMiddlewareCookieService : ICookieService
//    {
//        private IOwinContext _context;

//        public CookieMiddlewareCookieService(IOwinContext context)
//        {
//            _context = context;
//        }

//        public void AddValue(string value)
//        {
            
//        }

//        public IEnumerable<string> GetValuesAndDeleteCookie()
//        {
//            throw new NotImplementedException();
//        }

//        IEnumerable<string> GetValues()
//        {
//            var result = _context.Authentication.AuthenticateAsync(WsFederationConfiguration.WsFedCookieAuthenticationType).Result;

//        }
//    }
//}