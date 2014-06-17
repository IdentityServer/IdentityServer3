/*
 * Copyright (c) Damian Hickey.  All rights reserved.
 * see license
 */
namespace Thinktecture.IdentityServer.Core.Logging
{
    public interface ILogProvider
    {
        ILog GetLogger(string name);
    }
}