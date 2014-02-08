using System.Collections.Generic;
using System.Linq;
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
            var profileData = new Dictionary<string, object>();
            var claims = _profile.GetProfileData(
                request.AccessToken.Claims.First(c => c.Type == Constants.ClaimTypes.Subject).Value);
            
            foreach (var claim in claims)
            {
                profileData.Add(claim.Type, claim.Value);
            }

            return profileData;
        }
    }
}