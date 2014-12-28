/*
 * Copyright 2014 Dominick Baier, Brock Allen
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
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Diagnostics;
using Thinktecture.IdentityServer.Core.Events;
using Thinktecture.IdentityServer.Core.Logging;

namespace Thinktecture.IdentityServer.Core.Services.Default
{
    public class DefaultEventService : IEventService
    {
        protected static readonly ILog Logger = LogProvider.GetLogger("Events");

        private readonly IRequestIdService _reqId;
        private readonly OwinContext context;
        
        public DefaultEventService(OwinEnvironmentService owinEnvironment, IRequestIdService reqId)
        {
            this.context = new OwinContext(owinEnvironment.Environment);
            _reqId = reqId;
        }

        public void Raise(EventBase evt)
        {
            evt = PrepareEvent(evt);

            var json = LogSerializer.Serialize(evt);
            Logger.Info(json);
        }

        protected virtual EventBase PrepareEvent(EventBase evt)
        {
            if (evt == null) throw new ArgumentNullException("evt");

            evt.Context = new EventContext
            {
                ActivityId = _reqId.GetRequestId(),
                TimeStamp = DateTime.UtcNow,
                ProcessId = Process.GetCurrentProcess().Id,
                MachineName = Environment.MachineName,
                RemoteIpAddress = context.Request.RemoteIpAddress
            };

            return evt;
        }
    }
}