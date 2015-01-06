/*
 * Copyright 2014, 2015 Dominick Baier, Brock Allen
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *   http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using Microsoft.Owin;
using System;
using System.Diagnostics;
using Thinktecture.IdentityServer.Core.Configuration;
using Thinktecture.IdentityServer.Core.Events;
using Thinktecture.IdentityServer.Core.Logging;

namespace Thinktecture.IdentityServer.Core.Services.Default
{
    internal class EventServiceDecorator : IEventService
    {
        protected static readonly ILog Logger = LogProvider.GetLogger("Events");

        private readonly IdentityServerOptions options;
        private readonly IRequestIdService reqId;
        private readonly OwinContext context;
        private readonly IEventService inner;

        public EventServiceDecorator(IdentityServerOptions options, OwinEnvironmentService owinEnvironment, IRequestIdService reqId, IEventService inner)
        {
            this.options = options;
            this.context = new OwinContext(owinEnvironment.Environment);
            this.reqId = reqId;
            this.inner = inner;
        }

        public void Raise(EventBase evt)
        {
            if (CanRaiseEvent(evt))
            {
                evt = PrepareEvent(evt);
                inner.Raise(evt);
            }
        }

        bool CanRaiseEvent(EventBase evt)
        {
            switch(evt.EventType)
            {
                case EventType.Failure:
                    return options.EventsOptions.RaiseFailureEvents;
                case EventType.Information:
                    return options.EventsOptions.RaiseInformationEvents;
                case EventType.Success:
                    return options.EventsOptions.RaiseSuccessEvents;
                case EventType.Error:
                    return options.EventsOptions.RaiseErrorEvents;
            }

            return false;
        }

        protected virtual EventBase PrepareEvent(EventBase evt)
        {
            if (evt == null) throw new ArgumentNullException("evt");

            evt.Context = new EventContext
            {
                ActivityId = reqId.GetRequestId(),
                TimeStamp = DateTimeOffset.UtcNow,
                ProcessId = Process.GetCurrentProcess().Id,
                MachineName = Environment.MachineName,
                RemoteIpAddress = context.Request.RemoteIpAddress,
            };

            var principal = context.Authentication.User;
            if (principal != null && principal.Identity != null)
            {
                var subjectClaim = principal.FindFirst(Constants.ClaimTypes.Subject);
                if (subjectClaim != null)
                {
                    evt.Context.SubjectId = subjectClaim.Value;
                }
            }

            return evt;
        }
    }
}