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

        static readonly JsonSerializerSettings jsonSettings = new JsonSerializerSettings
        {
            DefaultValueHandling = DefaultValueHandling.Ignore,
            NullValueHandling = NullValueHandling.Ignore,
            DateFormatHandling = DateFormatHandling.IsoDateFormat,
            Formatting = Formatting.Indented,
        };

        static DefaultEventService()
        {
            jsonSettings.Converters.Add(new StringEnumConverter());
        }

        readonly OwinContext context;

        public DefaultEventService(OwinEnvironmentService owinEnvironment)
        {
            if (owinEnvironment == null) throw new ArgumentNullException("owinEnvironment");

            this.context = new OwinContext(owinEnvironment.Environment);
        }

        public void Raise(EventBase evt)
        {
            if (evt == null) throw new ArgumentNullException("evt");

            evt.Context = new EventContext
            {
                ActivityId = ActivityId.GetCurrentId(),
                TimeStamp = DateTime.UtcNow,
                ProcessId = Process.GetCurrentProcess().Id,
                MachineName = Environment.MachineName,
                RemoteIpAddress = context.Request.RemoteIpAddress
            };
            
            var json = JsonConvert.SerializeObject(evt, jsonSettings);
            Logger.Info(json);
        }
    }
}