/*
 * Copyright (c) Dominick Baier, Brock Allen.  All rights reserved.
 * see license
 */

using System;
using Thinktecture.IdentityServer.Core.Configuration;
using Thinktecture.IdentityServer.Core.Logging;
using Thinktecture.IdentityServer.Core.Services;
using Thinktecture.IdentityServer.WsFederation.Services;

namespace Thinktecture.IdentityServer.WsFederation.Configuration
{
    public class WsFederationServiceFactory
    {
        private static ILog Logger = LogProvider.GetCurrentClassLogger();

        

        // mandatory (external)
        public Registration<IUserService> UserService { get; set; }
        public Registration<WsFederationSettings> WsFederationSettings { get; set; }
        public Registration<IRelyingPartyService> RelyingPartyService { get; set; }

        public void Validate()
        {
            if (UserService == null) LogAndStop("UserService not configured");
            if (WsFederationSettings == null) LogAndStop("WsFederationSettings not configured");
            if (RelyingPartyService == null) LogAndStop("RelyingPartyService not configured");
        }

        private void LogAndStop(string message)
        {
            Logger.Error(message);
            throw new InvalidOperationException(message);
        }
    }
}