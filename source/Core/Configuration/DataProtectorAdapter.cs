using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Thinktecture.IdentityServer.Core.Configuration
{
    class DataProtectorAdapter : Microsoft.Owin.Security.DataProtection.IDataProtector
    {
        private readonly IDataProtector _idsrvProtector;
        private string _entropy;

        public DataProtectorAdapter(IDataProtector idsrvProtector, string entropy)
        {
            _idsrvProtector = idsrvProtector;
            _entropy = entropy;
        }

        public byte[] Protect(byte[] userData)
        {
            return _idsrvProtector.Protect(userData, _entropy);
        }

        public byte[] Unprotect(byte[] protectedData)
        {
            return _idsrvProtector.Unprotect(protectedData, _entropy);
        }
    }
}
