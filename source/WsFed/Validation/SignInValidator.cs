/*
 * Copyright (c) Dominick Baier, Brock Allen.  All rights reserved.
 * see license
 */
using System.IdentityModel.Services;
using System.Security.Claims;
using System.Threading.Tasks;
using Thinktecture.IdentityServer.Core;
using Thinktecture.IdentityServer.Core.Logging;
using Thinktecture.IdentityServer.WsFederation.Services;

namespace Thinktecture.IdentityServer.WsFederation.Validation
{
    public class SignInValidator
    {
        private readonly static ILog Logger = LogProvider.GetCurrentClassLogger();
        private readonly IRelyingPartyService _relyingParties;

        public SignInValidator(IRelyingPartyService relyingParties)
        {
            _relyingParties = relyingParties;
        }

        public async Task<SignInValidationResult> ValidateAsync(SignInRequestMessage message, ClaimsPrincipal subject)
        {
            Logger.Info("Validating WS-Federation signin request");
            var result = new SignInValidationResult();

            if (message.HomeRealm.IsPresent())
            {
                Logger.Info("Setting home realm to: " + message.HomeRealm);
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
                Logger.Error("Relying party not found: " + rp.Realm);

                return new SignInValidationResult
                {
                    IsError = true,
                    Error = "invalid_relying_party"
                };
            }

            Logger.InfoFormat("Relying party registration found: {0} / {1}", rp.Realm, rp.Name);

            result.ReplyUrl = rp.ReplyUrl;
            Logger.InfoFormat("Reply URL set to: " + result.ReplyUrl);

            result.RelyingParty = rp;
            result.SignInRequestMessage = message;
            result.Subject = subject;

            return result;
        }
    }
}