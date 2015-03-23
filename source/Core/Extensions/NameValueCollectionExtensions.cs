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

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Text;
using Encoder = Microsoft.Security.Application.Encoder;

namespace Thinktecture.IdentityServer.Core.Extensions
{
    internal static class NameValueCollectionExtensions
    {
        public static string ToQueryString(this NameValueCollection collection)
        {
            if (collection.Count == 0)
            {
                return String.Empty;
            }

            var builder = new StringBuilder(128);
            var first = true;
            foreach (string name in collection) {
                var values = collection.GetValues(name);
                first = values == null || values.Length == 0
                    ? AppendNameValuePair(builder, first, true, name, String.Empty)
                    : values.Aggregate(first,
                        (current, value) => AppendNameValuePair(builder, current, true, name, value));
            }

            return builder.ToString();
        }

        public static string ToFormPost(this NameValueCollection collection)
        {
            var builder = new StringBuilder(128);
            const string inputFieldFormat = "<input type=\"hidden\" name=\"{0}\" value=\"{1}\" />\n";

            foreach (string name in collection)
            {
                var values = collection.GetValues(name);
                var value = values.First();
                value = Encoder.HtmlEncode(value);
                builder.AppendFormat(inputFieldFormat, name, value);
            }

            return builder.ToString();
        }


        public static Dictionary<string, string> ToDictionary(this NameValueCollection collection)
        {
            var dict = new Dictionary<string, string>();
            
            if (collection == null || collection.Count == 0)
            {
                return dict;
            }

            foreach (string name in collection)
            {
                var value = collection.Get(name);
                if (value != null)
                {
                    dict.Add(name, value);
                }
            }

            return dict;
        }

        internal static string ConvertFormUrlEncodedSpacesToUrlEncodedSpaces(string str)
        {
            if ((str != null) && (str.IndexOf('+') >= 0))
            {
                str = str.Replace("+", "%20");
            }
            return str;
        }

        private static bool AppendNameValuePair(StringBuilder builder, bool first, bool urlEncode, string name, string value)
        {
            var effectiveName = name ?? String.Empty;
            var encodedName = urlEncode ? WebUtility.UrlEncode(effectiveName) : effectiveName;

            var effectiveValue = value ?? String.Empty;
            var encodedValue = urlEncode ? WebUtility.UrlEncode(effectiveValue) : effectiveValue;
            encodedValue = ConvertFormUrlEncodedSpacesToUrlEncodedSpaces(encodedValue);

            if (first)
            {
                first = false;
            }
            else
            {
                builder.Append("&");
            }

            builder.Append(encodedName);
            if (!String.IsNullOrEmpty(encodedValue))
            {
                builder.Append("=");
                builder.Append(encodedValue);
            }
            return first;
        }
    }
}