/*
 * Copyright (c) Dominick Baier, Brock Allen.  All rights reserved.
 * see license
 */

using System;
using System.Diagnostics;

namespace Thinktecture.IdentityServer.Core
{
    public static class DateTimeExtensions
    {
        [DebuggerStepThrough]
        public static bool HasExceeded(this DateTime creationTime, int seconds)
        {
            return (DateTime.UtcNow > creationTime.AddSeconds(seconds));
        }

        [DebuggerStepThrough]
        public static int GetLifetimeInSeconds(this DateTime creationTime)
        {
            // todo: double?
            return ((int)(DateTime.UtcNow- creationTime).TotalSeconds);
        }
    }
}
