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

using System;

namespace Thinktecture.IdentityServer.Core.Events
{
    public class EventBase
    {
        public int Id { get; set; }
        public string ActivityId { get; set; }
        public DateTime TimeStamp { get; set; }
        public EventType EventType { get; set; }
        public string Category { get; set; }

        public int ProcessId { get; set; }
        public string MachineName { get; set; }
        public string RemoteIpAddress { get; set; }
        
        public string Message { get; set; }
        public string Details { get; set; }
    }
}