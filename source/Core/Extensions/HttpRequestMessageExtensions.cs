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

using System.Collections.Generic;
using System.Linq;
using System.Net.Http;

namespace IdentityServer3.Core.Extensions
{
    internal static class HttpRequestMessageExtensions
    {
        public static string GetIdentityServerBaseUrl(this HttpRequestMessage request)
        {
            return request.GetOwinContext().Environment.GetIdentityServerBaseUrl();
        }


        const string SuppressXfo = "idsvr:SuppressXfo";

        public static void SetSuppressXfo(this HttpRequestMessage request)
        {
            request.Properties[SuppressXfo] = true;
        }

        public static bool GetSuppressXfo(this HttpRequestMessage request)
        {
            return request.Properties.ContainsKey(SuppressXfo) && true.Equals(request.Properties[SuppressXfo]);
        }


        const string AllowedCspFrameOrigins = "idsvr:AllowedCspFrameOrigins";

        public static void SetAllowedCspFrameOrigins(this HttpRequestMessage request, IEnumerable<string> origins)
        {
            request.Properties[AllowedCspFrameOrigins] = origins;
        }

        public static IEnumerable<string> GetAllowedCspFrameOrigins(this HttpRequestMessage request)
        {
            if (request.Properties.ContainsKey(AllowedCspFrameOrigins))
            {
                return (IEnumerable<string>)request.Properties[AllowedCspFrameOrigins];
            }

            return Enumerable.Empty<string>();
        }
    }
}