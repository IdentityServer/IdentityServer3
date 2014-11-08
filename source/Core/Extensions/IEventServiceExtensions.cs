using Microsoft.Owin;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Thinktecture.IdentityServer.Core.Events;
using Thinktecture.IdentityServer.Core.Models;
using Thinktecture.IdentityServer.Core.Services;

namespace Thinktecture.IdentityServer.Core.Extensions
{
    internal static class IEventServiceExtensions
    {
        public static void RaiseLocalLoginSuccessEvent(this IEventService events, IDictionary<string, object> env, string username, SignInMessage signInMessage, AuthenticateResult authResult)
        {
            if (events == null) throw new ArgumentNullException("events");

            var evt = new LocalAuthenticationEvent()
            {
                Id = EventConstants.Ids.SuccessfulLocalLogin,
                EventType = Events.EventType.Success,
                Message = Resources.Events.LocalLoginSuccess,
                SubjectId = authResult.User.GetSubjectId(),
                SignInMessage = signInMessage,
                LoginUserName = username
            };
            evt.ApplyEnvironment(env);
            events.Raise(evt);
        }

        internal static void ApplyEnvironment(this EventBase evt, IDictionary<string, object> env)
        {
            if (env == null) throw new ArgumentNullException("env");

            var ctx = new OwinContext(env);

            evt.ActivityId = GetActivityId();
            evt.TimeStamp = DateTime.UtcNow;
            evt.ProcessId = Process.GetCurrentProcess().Id;
            evt.MachineName = Environment.MachineName; 
            evt.RemoteIpAddress = ctx.Request.RemoteIpAddress;
        }

        internal static string GetActivityId()
        {
            if (Trace.CorrelationManager.ActivityId == Guid.Empty)
            {
                Trace.CorrelationManager.ActivityId = Guid.NewGuid();
            }

            return Trace.CorrelationManager.ActivityId.ToString("N");
        }
    }
}
