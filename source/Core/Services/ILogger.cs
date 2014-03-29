/*
 * Copyright (c) Dominick Baier, Brock Allen.  All rights reserved.
 * see license
 */

namespace Thinktecture.IdentityServer.Core.Services
{
    public interface ILogger
    {
        void Verbose(string message);
        void Information(string message);
        void Warning(string message);
        void Error(string message);

        void Start(string action);
    }
}
