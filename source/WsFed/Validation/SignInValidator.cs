using System.IdentityModel.Services;
using System.Security.Claims;
using Thinktecture.IdentityServer.Core.Services;
using Thinktecture.IdentityServer.WsFed.Services;

namespace Thinktecture.IdentityServer.WsFed.Validation
{
    public class SignInValidator
    {
        private ILogger _logger;
        private TestRelyingPartyService _relyingParties;

        public SignInValidator(ILogger logger)
        {
            _logger = logger;

            // todo: DI
            _relyingParties = new TestRelyingPartyService();
        }

        public SignInValidationResult Validate(SignInRequestMessage message, ClaimsPrincipal subject)
        {
            var result = new SignInValidationResult();

            // todo: wfresh handling?
            if (!subject.Identity.IsAuthenticated)
            {
                return new SignInValidationResult
                {
                    IsSignInRequired = true,
                };
            };

            var rp = _relyingParties.GetByRealm(message.Realm);

            if (rp == null || rp.Enabled == false)
            {
                return new SignInValidationResult
                {
                    IsError = true,
                    Error = "invalid_relying_party"
                };
            }

            // todo: check wreply against list of allowed reply URLs
            result.ReplyUrl = rp.ReplyUrl;

            result.RelyingParty = rp;
            result.SignInRequestMessage = message;
            result.Subject = subject;

            return result;
        }
    }
}