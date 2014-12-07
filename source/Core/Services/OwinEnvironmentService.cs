using Microsoft.Owin;
using System.Collections.Generic;

namespace Thinktecture.IdentityServer.Core.Services
{
    public class OwinEnvironmentService
    {
        readonly IOwinContext _context;

        public OwinEnvironmentService(IOwinContext context)
        {
            _context = context;
        }

        public IDictionary<string, object> Environment 
        { 
            get
            {
                return _context.Environment;
            }
        }
    }
}