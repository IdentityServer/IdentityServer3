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

using IdentityModel;
using IdentityServer3.Core.Configuration;
using System.Text;

namespace IdentityServer3.Core.Extensions
{
    /// <summary>
    /// Extension methods for <see cref="IdentityServer3.Core.Configuration.IDataProtector"/>
    /// </summary>
    public static class IDataProtectorExtensions
    {
        /// <summary>
        /// Protects the specified data and Base64 Url encodes the response.
        /// </summary>
        /// <param name="protector">The protector.</param>
        /// <param name="data">The data.</param>
        /// <param name="entropy">The entropy.</param>
        /// <returns></returns>
        public static string Protect(this IDataProtector protector, string data, string entropy = "")
        {
            var dataBytes = Encoding.UTF8.GetBytes(data);
            var protectedBytes = protector.Protect(dataBytes, entropy);

            return Base64Url.Encode(protectedBytes);
        }

        /// <summary>
        /// Base64 Url decodes the input and unprotects the data.
        /// </summary>
        /// <param name="protector">The protector.</param>
        /// <param name="data">The data.</param>
        /// <param name="entropy">The entropy.</param>
        /// <returns></returns>
        public static string Unprotect(this IDataProtector protector, string data, string entropy = "")
        {
            var protectedBytes = Base64Url.Decode(data);
            var bytes = protector.Unprotect(protectedBytes, entropy);

            return Encoding.UTF8.GetString(bytes);
        }
    }
}