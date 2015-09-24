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

using IdentityServer3.Core.Models;
using System.Collections.Specialized;

namespace IdentityServer3.Core.Extensions
{
    internal static class AuthorizeResponseExtensions
    {
        public static NameValueCollection ToNameValueCollection(this AuthorizeResponse response)
        {
            var collection = new NameValueCollection();

            if (response.IsError)
            {
                if (response.Error.IsPresent())
                {
                    collection.Add("error", response.Error);
                }
            }
            else
            {
                if (response.Code.IsPresent())
                {
                    collection.Add("code", response.Code);
                }

                if (response.IdentityToken.IsPresent())
                {
                    collection.Add("id_token", response.IdentityToken);
                }

                if (response.AccessToken.IsPresent())
                {
                    collection.Add("access_token", response.AccessToken);
                    collection.Add("token_type", "Bearer");
                    collection.Add("expires_in", response.AccessTokenLifetime.ToString());
                }

                if (response.Scope.IsPresent())
                {
                    collection.Add("scope", response.Scope);
                }
            }

            if (response.State.IsPresent())
            {
                collection.Add("state", response.State);
            }
            
            if (response.SessionState.IsPresent())
            {
                collection.Add("session_state", response.SessionState);
            }

            return collection;
        }
    }
}