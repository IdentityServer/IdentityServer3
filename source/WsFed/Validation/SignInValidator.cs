/*
 * Copyright (c) Dominick Baier, Brock Allen.  All rights reserved.
 * see license
 */
using System.IdentityModel.Services;
using System.Security.Claims;
using System.Threading.Tasks;
using Thinktecture.IdentityServer.Core.Services;
using Thinktecture.IdentityServer.WsFed.Services;
using Thinktecture.IdentityServer.Core;

namespace Thinktecture.IdentityServer.WsFed.Validation
{
    public class SignInValidator
    {
        private readonly ILogger _logger;
        private readonly IRelyingPartyService _relyingParties;

        public SignInValidator(ILogger logger, IRelyingPartyService relyingParties)
        {
            _logger = logger;
            _relyingParties = relyingParties;
        }

        public async Task<SignInValidationResult> ValidateAsync(SignInRequestMessage message, ClaimsPrincipal subject)
        {
            var result = new SignInValidationResult();

            if (message.HomeRealm.IsPresent())
            {
                result.HomeRealm = message.HomeRealm;
            }

            // todo: wfresh handling?
            if (!subject.Identity.IsAuthenticated)
            {
                result.IsSignInRequired = true;
                return result;
            };

            var rp = await _relyingParties.GetByRealmAsync(message.Realm);

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