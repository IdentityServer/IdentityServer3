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

using IdentityServer3.Core.Extensions;
using IdentityServer3.Core.Models;
using IdentityServer3.Core.Validation;
using System.ComponentModel;
using System.Linq;

#pragma warning disable 1591

namespace IdentityServer3.Core.ResponseHandling
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class EndSessionResponseGenerator
    {
        public SignOutMessage CreateSignoutMessage(ValidatedEndSessionRequest request)
        {
            var message = new SignOutMessage();
            
            if (request.Client != null)
            {
                message.ClientId = request.Client.ClientId;

                if (request.PostLogOutUri != null)
                {
                    message.ReturnUrl = request.PostLogOutUri;
                }
                else
                {
                    if (request.Client.PostLogoutRedirectUris.Any())
                    {
                        message.ReturnUrl = request.Client.PostLogoutRedirectUris.First();
                    }
                }

                if (request.State.IsPresent())
                {
                    if (message.ReturnUrl.IsPresent())
                    {
                        message.ReturnUrl = message.ReturnUrl.AddQueryString("state=" + request.State);
                    }
                }
            }

            return message;
        }
    }
}