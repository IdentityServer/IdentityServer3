﻿/*
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

using Thinktecture.IdentityServer.Core.Authentication;

namespace Thinktecture.IdentityServer.Core.Connect.Models
{
    public abstract class InteractionResponse
    {
        public bool IsError { get { return Error != null; } }
        public AuthorizeError Error { get; set; }
    }

    public class LoginInteractionResponse : InteractionResponse
    {
        public bool IsLogin { get { return SignInMessage != null; } }
        public SignInMessage SignInMessage { get; set; }
    }
    
    public class ConsentInteractionResponse : InteractionResponse
    {
        public bool IsConsent { get; set; }
        public string ConsentError { get; set; }
    }
}
