using IdentityServer3.Core.Validation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IdentityServer3.Core.Services
{
    /// <summary>
    /// Allows inserting custom client validation logic into authorize requests
    /// </summary>
    public interface ICustomClientValidator
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        Task<AuthorizeRequestValidationResult> ValidateAuthorizeRequestAsync(ValidatedAuthorizeRequest request);

    }
}
