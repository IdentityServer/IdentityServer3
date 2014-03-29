/*
 * Copyright (c) Dominick Baier, Brock Allen.  All rights reserved.
 * see license
 */

namespace Thinktecture.IdentityServer.Core.Services
{
    public interface ISettingsService
    {
        string Prefix { get; set; }

        string Get(string name);
        void Set(string name, string value);
    }
}
