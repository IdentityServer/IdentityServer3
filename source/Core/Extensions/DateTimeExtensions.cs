using System;
using System.Diagnostics;

namespace Thinktecture.IdentityServer.Core
{
    public static class DateTimeExtensions
    {
        [DebuggerStepThrough]
        public static bool HasExpired(this DateTime creationTime, int seconds)
        {
            return (DateTime.UtcNow > creationTime.AddSeconds(seconds));
        }
    }
}
