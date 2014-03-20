using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Thinktecture.IdentityServer.Admin.Core
{
    public class UserManagerResult<T> : UserManagerResult
    {
        public UserManagerResult(T result)
        {
            Result = result;
        }
        
        public UserManagerResult(params string[] errors)
            : base(errors)
        {
        }

        public T Result { get; private set; }
    }
}
