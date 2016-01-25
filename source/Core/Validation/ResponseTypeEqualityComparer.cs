using System;
using System.Collections.Generic;

namespace IdentityServer3.Core.Validation
{
    /// <summary>
    /// Compares resource_type strings, where the order of space-delimited values is insignificant.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This is to handle the fact that the order of multi-valued response_type lists is
    /// insignificant, per the <see href="https://tools.ietf.org/html/rfc6749#section-3.1.1">OAuth2 spec</see>
    /// and the 
    /// (<see href="http://openid.net/specs/oauth-v2-multiple-response-types-1_0-03.html#terminology">OAuth 
    /// 2.0 Multiple Response Type Encoding Practices draft </see>).
    /// </para>
    /// </remarks>
    public class ResponseTypeEqualityComparer : IEqualityComparer<string>
    {
        /// <summary>
        /// Determines whether the specified values are equal.
        /// </summary>
        /// <param name="x">The first string to compare.</param>
        /// <param name="y">The second string to compare.</param>
        /// <returns>true if the specified values are equal; otherwise, false.</returns>
        public bool Equals(string x, string y)
        {
            if (x == y) return true;

            if (x == null || y == null) return false;

            if (x.Length != y.Length) return false;

            var xValues = x.Split(' ');
            var yValues = y.Split(' ');

            if (xValues.Length != yValues.Length)
            {
                return false;
            }

            Array.Sort(xValues);
            Array.Sort(yValues);

            for (int i = 0; i < xValues.Length; i++)
            {
                if (xValues[i] != yValues[i])
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Returns a hash code for the value.
        /// </summary>
        /// <param name="value">The value for which a hash code is to be returned.</param>
        /// <returns>A hash code for the value, suitable for use in hashing algorithms and data structures like a hash table.</returns>
        public int GetHashCode(string value)
        {
            if (value == null) return 0;

            var values = value.Split(' ');
            if (values.Length == 1)
            {
                // Only one value, so just spit out the hash code of the whole string
                return value.GetHashCode();
            }

            Array.Sort(values);

            // Using Skeet's answer here: http://stackoverflow.com/a/7244729/208990
            int hash = 17;
            foreach (var element in values)
            {
                hash = hash * 31 + StringComparer.InvariantCulture.GetHashCode(element);
            }
            return hash;
        }
    }
}
