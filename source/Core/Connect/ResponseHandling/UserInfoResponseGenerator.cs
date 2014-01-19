using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Thinktecture.IdentityServer.Core.Services;

namespace Thinktecture.IdentityServer.Core.Connect
{
    public class UserInfoResponseGenerator
    {
        private IUserService _profile;

        public UserInfoResponseGenerator(IUserService profile)
        {
            _profile = profile;
        }

        public Dictionary<string, object> Process(ValidatedUserInfoRequest request)
        {
            return null;
        }
    }
}
