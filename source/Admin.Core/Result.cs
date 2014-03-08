using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Thinktecture.IdentityServer.Admin.Core
{
    public class Result
    {
        public static readonly Result Success = new Result();
        
        public Result()
        {
        }
        public Result(IEnumerable<string> errors)
        {
            if (errors == null) throw new ArgumentNullException("errors");
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
