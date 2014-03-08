using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Thinktecture.IdentityServer.Admin.Core
{
    public class UserManagerResult
    {
        public static readonly UserManagerResult Success = new UserManagerResult();
        
        public UserManagerResult(params string[] errors)
        {
            this.Errors = errors;
        }

        public IEnumerable<string> Errors { get; private set; }
        
        public bool IsSuccess
        {
            get { return Errors == null || !Errors.Any(); }
        }

        public bool IsError
        {
            get { return !IsSuccess; }
        }
    }
}
