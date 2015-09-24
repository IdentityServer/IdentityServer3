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
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;

namespace IdentityServer3.Tests.Conformance
{
    public class CookieHeaderValue
    {
        public Collection<CookieState> Cookies = new Collection<CookieState>();
        public string Domain { get; set; }
        public DateTimeOffset? Expires { get; set; }
        public bool HttpOnly { get; set; }
        public TimeSpan? MaxAge { get; set; }
        public string Path { get; set; }
        public bool Secure { get; set; }


        public static bool TryParse(string input, out CookieHeaderValue parsedValue)
        {
            var segmentSeparator = new char[] { ';' };

            parsedValue = null;
            if (string.IsNullOrEmpty(input))
            {
                return false;
            }
            string[] strArray = input.Split(segmentSeparator);
            CookieHeaderValue instance = new CookieHeaderValue();
            foreach (string str in strArray)
            {
                if (!ParseCookieSegment(instance, str))
                {
                    return false;
                }
            }
            if (instance.Cookies.Count == 0)
            {
                return false;
            }
            parsedValue = instance;
            return true;
        }

        private static bool TryParseInt32(string value, out int result)
        {
            return int.TryParse(value, System.Globalization.NumberStyles.None, System.Globalization.NumberFormatInfo.InvariantInfo, out result);
        }

        private static bool TryParseDate(string input, out DateTimeOffset result)
        {
            var dateFormats = new string[] { 
                "ddd, d MMM yyyy H:m:s 'GMT'", "ddd, d MMM yyyy H:m:s", 
                "d MMM yyyy H:m:s 'GMT'", "d MMM yyyy H:m:s", 
                "ddd, d MMM yy H:m:s 'GMT'", "ddd, d MMM yy H:m:s", 
                "d MMM yy H:m:s 'GMT'", "d MMM yy H:m:s", 
                "dddd, d'-'MMM'-'yy H:m:s 'GMT'", "dddd, d'-'MMM'-'yy H:m:s", 
                "ddd MMM d H:m:s yyyy", "ddd, d MMM yyyy H:m:s zzz", "ddd, d MMM yyyy H:m:s", 
                "d MMM yyyy H:m:s zzz", "d MMM yyyy H:m:s",
            };

            if (DateTimeOffset.TryParse(input, out result)) return true;
            return DateTimeOffset.TryParseExact(input, dateFormats, System.Globalization.DateTimeFormatInfo.InvariantInfo, System.Globalization.DateTimeStyles.AssumeUniversal | System.Globalization.DateTimeStyles.AllowWhiteSpaces, out result);
        }

        private static bool ParseCookieSegment(CookieHeaderValue instance, string segment)
        {
            var nameValueSeparator = new char[] { '=' };

            if (string.IsNullOrWhiteSpace(segment))
            {
                return true;
            }
            string[] nameValuePair = segment.Split(nameValueSeparator, 2);
            if ((nameValuePair.Length < 1) || string.IsNullOrWhiteSpace(nameValuePair[0]))
            {
                return false;
            }
            string a = nameValuePair[0].Trim();
            if (string.Equals(a, "expires", StringComparison.OrdinalIgnoreCase))
            {
                DateTimeOffset offset;
                if (TryParseDate(GetSegmentValue(nameValuePair, null), out offset))
                {
                    instance.Expires = new DateTimeOffset?(offset);
                    return true;
                }
                return false;
            }
            if (string.Equals(a, "max-age", StringComparison.OrdinalIgnoreCase))
            {
                int num;
                if (TryParseInt32(GetSegmentValue(nameValuePair, null), out num))
                {
                    instance.MaxAge = new TimeSpan(0, 0, num);
                    return true;
                }
                return false;
            }
            if (string.Equals(a, "domain", StringComparison.OrdinalIgnoreCase))
            {
                instance.Domain = GetSegmentValue(nameValuePair, null);
                return true;
            }
            if (string.Equals(a, "path", StringComparison.OrdinalIgnoreCase))
            {
                instance.Path = GetSegmentValue(nameValuePair, "/");
                return true;
            }
            if (string.Equals(a, "secure", StringComparison.OrdinalIgnoreCase))
            {
                if (!string.IsNullOrWhiteSpace(GetSegmentValue(nameValuePair, null)))
                {
                    return false;
                }
                instance.Secure = true;
                return true;
            }
            if (string.Equals(a, "httponly", StringComparison.OrdinalIgnoreCase))
            {
                if (!string.IsNullOrWhiteSpace(GetSegmentValue(nameValuePair, null)))
                {
                    return false;
                }
                instance.HttpOnly = true;
                return true;
            }
            string segmentValue = GetSegmentValue(nameValuePair, null);
            try
            {
                NameValueCollection values = new FormDataCollection(segmentValue).ReadAsNameValueCollection();
                CookieState item = new CookieState(a, values);
                instance.Cookies.Add(item);
                return true;
            }
            catch
            {
                return false;
            }
        }

        private static string GetSegmentValue(string[] nameValuePair, string defaultValue)
        {
            if (nameValuePair.Length <= 1)
            {
                return defaultValue;
            }
            return UnquoteToken(nameValuePair[1]);
        }

        private static string UnquoteToken(string token)
        {
            if (!string.IsNullOrWhiteSpace(token) && ((token.StartsWith("\"", StringComparison.Ordinal) && token.EndsWith("\"", StringComparison.Ordinal)) && (token.Length > 1)))
            {
                return token.Substring(1, token.Length - 2);
            }
            return token;
        }
    }
}
