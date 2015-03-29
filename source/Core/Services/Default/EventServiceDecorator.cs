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

using IdentityServer3.Core.Configuration;
using IdentityServer3.Core.Events;
using IdentityServer3.Core.Extensions;
using IdentityServer3.Core.Logging;
using Microsoft.Owin;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace IdentityServer3.Core.Services.Default
{
    internal class EventServiceDecorator : IEventService
    {
        protected static readonly ILog Logger = LogProvider.GetLogger("Events");

        private readonly IdentityServerOptions options;
        private readonly OwinContext context;
        private readonly IEventService inner;

        public EventServiceDecorator(IdentityServerOptions options, OwinEnvironmentService owinEnvironment, IEventService inner)
        {
            this.options = options;
            this.context = new OwinContext(owinEnvironment.Environment);
            this.inner = inner;
        }

        public Task RaiseAsync<T>(Event<T> evt)
        {
            if (CanRaiseEvent(evt))
            {
                evt = PrepareEvent(evt);
                evt.Prepare();
                inner.RaiseAsync(evt);
            }

            return Task.FromResult(0);
        }

        bool CanRaiseEvent<T>(Event<T> evt)
        {
            switch(evt.EventType)
            {
                case EventTypes.Failure:
                    return options.EventsOptions.RaiseFailureEvents;
                case EventTypes.Information:
                    return options.EventsOptions.RaiseInformationEvents;
                case EventTypes.Success:
                    return options.EventsOptions.RaiseSuccessEvents;
                case EventTypes.Error:
                    return options.EventsOptions.RaiseErrorEvents;
            }

            return false;
        }

        protected virtual Event<T> PrepareEvent<T>(Event<T> evt)
        {
            if (evt == null) throw new ArgumentNullException("evt");

            evt.Context = new EventContext
            {
                ActivityId = context.GetRequestId(),
                TimeStamp = DateTimeOffsetHelper.UtcNow,
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