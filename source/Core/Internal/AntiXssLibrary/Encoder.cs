// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Encoder.cs" company="Microsoft Corporation">
//   Copyright (c) 2008, 2009, 2010 All Rights Reserved, Microsoft Corporation
//
//   This source is subject to the Microsoft Permissive License.
//   Please see the License.txt file for more information.
//   All other rights reserved.
//
//   THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY 
//   KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
//   IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
//   PARTICULAR PURPOSE.
//
// </copyright>
// <summary>
//   Performs encoding of input strings to provide protection against
//   Cross-Site Scripting (XSS) attacks and LDAP injection attacks in
//   various contexts.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

// ReSharper disable CheckNamespace
namespace Microsoft.Security.Application
    // ReSharper restore CheckNamespace
{

    /// <summary>
    /// Performs encoding of input strings to provide protection against
    /// Cross-Site Scripting (XSS) attacks and LDAP injection attacks in 
    /// various contexts.
    /// </summary>
    /// <remarks>
    /// This encoding library uses the Principle of Inclusions, 
    /// sometimes referred to as "safe-listing" to provide protection 
    /// against injection attacks.  With safe-listing protection, 
    /// algorithms look for valid inputs and automatically treat 
    /// everything outside that set as a potential attack.  This library 
    /// can be used as a defense in depth approach with other mitigation 
    /// techniques. It is suitable for applications with high security 
    /// requirements.
    /// </remarks>
    internal static class Encoder
    {
        /// <summary>
        /// Encodes input strings for use in HTML.
        /// </summary>
        /// <param name="input">String to be encoded.</param>
        /// <returns>
        /// Encoded string for use in HTML.
        /// </returns>
        /// <remarks>
        /// All characters not safe listed are encoded to their Unicode decimal value, using &amp;#DECIMAL; notation.
        /// The default safe characters include:
        /// <list type="table">
        /// <item><term>a-z</term><description>Lower case alphabet</description></item>
        /// <item><term>A-Z</term><description>Upper case alphabet</description></item>
        /// <item><term>0-9</term><description>Numbers</description></item>
        /// <item><term>,</term><description>Comma</description></item>
        /// <item><term>.</term><description>Period</description></item>
        /// <item><term>-</term><description>Dash</description></item>
        /// <item><term>_</term><description>Underscore</description></item>
        /// <item><term>'</term><description>Apostrophe</description></item>
        /// <item><term> </term><description>Space</description></item>
        /// </list>
        /// The safe list may be adjusted using <see cref="UnicodeCharacterEncoder.MarkAsSafe"/>.
        /// <newpara/>
        /// Example inputs and their related encoded outputs:
        /// <list type="table">
        /// <item><term>&lt;script&gt;alert('XSS Attack!');&lt;/script&gt;</term><description>&amp;lt;script&amp;gt;alert('XSS Attack!');&amp;lt;/script&amp;gt;</description></item>
        /// <item><term>user@contoso.com</term><description>user@contoso.com</description></item>
        /// <item><term>Anti-Cross Site Scripting Library</term><description>Anti-Cross Site Scripting Library</description></item>
        /// <item><term>"Anti-Cross Site Scripting Library"</term><description>&amp;quote;Anti-Cross Site Scripting Library&amp;quote;</description></item>
        /// </list>
        /// </remarks>
        public static string HtmlEncode(string input)
        {
            return HtmlEncode(input, false);
        }

        /// <summary>
        /// Encodes input strings for use in HTML.
        /// </summary>
        /// <param name="input">String to be encoded.</param>
        /// <param name="useNamedEntities">Value indicating if the HTML 4.0 named entities should be used.</param>
        /// <returns>
        /// Encoded string for use in HTML.
        /// </returns>
        /// <remarks>
        /// All characters not safe listed are encoded to their Unicode decimal value, using &amp;#DECIMAL; notation.
        /// If you choose to use named entities then if a character is an HTML4.0 named entity the named entity will be used.
        /// The default safe characters include:
        /// <list type="table">
        /// <item><term>a-z</term><description>Lower case alphabet</description></item>
        /// <item><term>A-Z</term><description>Upper case alphabet</description></item>
        /// <item><term>0-9</term><description>Numbers</description></item>
        /// <item><term>,</term><description>Comma</description></item>
        /// <item><term>.</term><description>Period</description></item>
        /// <item><term>-</term><description>Dash</description></item>
        /// <item><term>_</term><description>Underscore</description></item>
        /// <item><term>'</term><description>Apostrophe</description></item>
        /// <item><term> </term><description>Space</description></item>
        /// </list>
        /// The safe list may be adjusted using <see cref="UnicodeCharacterEncoder.MarkAsSafe"/>.
        /// <newpara/>
        /// Example inputs and their related encoded outputs:
        /// <list type="table">
        /// <item><term>&lt;script&gt;alert('XSS Attack!');&lt;/script&gt;</term><description>&amp;lt;script&amp;gt;alert('XSS Attack!');&amp;lt;/script&amp;gt;</description></item>
        /// <item><term>user@contoso.com</term><description>user@contoso.com</description></item>
        /// <item><term>Anti-Cross Site Scripting Library</term><description>Anti-Cross Site Scripting Library</description></item>
        /// <item><term>"Anti-Cross Site Scripting Library"</term><description>&amp;quote;Anti-Cross Site Scripting Library&amp;quote;</description></item>
        /// </list>
        /// </remarks>
        public static string HtmlEncode(string input, bool useNamedEntities)
        {
            return UnicodeCharacterEncoder.HtmlEncode(input, useNamedEntities);
        }
    }
}
