// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SafeList.cs" company="Microsoft Corporation">
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
//   Provides safe list utility functions.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

// ReSharper disable CheckNamespace

using System.Collections;
using System.Globalization;
using Microsoft.Security.Application.CodeCharts;
using Thinktecture.IdentityServer.Core.Internal.AntiXssLibrary.CodeCharts;

namespace Microsoft.Security.Application
    // ReSharper restore CheckNamespace
{
    /// <summary>
    /// Provides safe list utility functions.
    /// </summary>
    internal static class SafeList
    {
        /// <summary>
        /// Generates a safe character array representing the specified value.
        /// </summary>
        /// <returns>A safe character array representing the specified value.</returns>
        /// <param name="value">The value to generate a safe representation for.</param>
        internal delegate char[] GenerateSafeValue(int value);

        /// <summary>
        /// Generates a new safe list of the specified size, using the specified function to produce safe values.
        /// </summary>
        /// <param name="length">The length of the safe list to generate.</param>
        /// <param name="generateSafeValue">The <see cref="GenerateSafeValue"/> function to use.</param>
        /// <returns>A new safe list.</returns>
        internal static char[][] Generate(int length, GenerateSafeValue generateSafeValue)
        {
            var allCharacters = new char[length + 1][];
            for (var i = 0; i <= length; i++)
            {
                allCharacters[i] = generateSafeValue(i);
            }

            return allCharacters;
        }

        /// <summary>
        /// Marks characters from the specified languages as safe.
        /// </summary>
        /// <param name="safeList">The safe list to punch holes in.</param>
        /// <param name="lowerCodeCharts">The combination of lower code charts to use.</param>
        /// <param name="lowerMidCodeCharts">The combination of lower mid code charts to use.</param>
        /// <param name="midCodeCharts">The combination of mid code charts to use.</param>
        /// <param name="upperMidCodeCharts">The combination of upper mid code charts to use.</param>
        /// <param name="upperCodeCharts">The combination of upper code charts to use.</param>
        internal static void PunchUnicodeThrough(
            ref char[][] safeList,
            LowerCodeCharts lowerCodeCharts, 
            LowerMidCodeCharts lowerMidCodeCharts, 
            MidCodeCharts midCodeCharts, 
            UpperMidCodeCharts upperMidCodeCharts, 
            UpperCodeCharts upperCodeCharts)
        {
            if (lowerCodeCharts != LowerCodeCharts.NONE)
            {
                PunchCodeCharts(ref safeList, lowerCodeCharts);
            }

            if (lowerMidCodeCharts != LowerMidCodeCharts.NONE)
            {
                PunchCodeCharts(ref safeList, lowerMidCodeCharts);
            }

            if (midCodeCharts != MidCodeCharts.NONE)
            {
                PunchCodeCharts(ref safeList, midCodeCharts);
            }

            if (upperMidCodeCharts != UpperMidCodeCharts.NONE)
            {
                PunchCodeCharts(ref safeList, upperMidCodeCharts);
            }

            if (upperCodeCharts != UpperCodeCharts.NONE)
            {
                PunchCodeCharts(ref safeList, upperCodeCharts);
            }
        }

        /// <summary>
        /// Punches holes as necessary.
        /// </summary>
        /// <param name="safeList">The safe list to punch through.</param>
        /// <param name="whiteListedCharacters">The list of character positions to punch.</param>
        internal static void PunchSafeList(ref char[][] safeList, IEnumerable whiteListedCharacters)
        {
            PunchHolesIfNeeded(ref safeList, true, whiteListedCharacters);
        }

        /// <summary>
        /// Generates a hash prefixed character array representing the specified value.
        /// </summary>
        /// <param name="value">The source value.</param>
        /// <returns>A character array representing the specified value.</returns>
        /// <remarks>
        /// Example inputs and encoded outputs:
        /// <list type="table">
        /// <item><term>1</term><description>#1</description></item>
        /// <item><term>10</term><description>#10</description></item>
        /// <item><term>100</term><description>#100</description></item>
        /// </list>
        /// </remarks>
        internal static char[] HashThenValueGenerator(int value)
        {
            return StringToCharArrayWithHashPrefix(value.ToString(CultureInfo.InvariantCulture));
        }

        /// <summary>
        /// Generates a hash prefixed character array representing the specified value in hexadecimal.
        /// </summary>
        /// <param name="value">The source value.</param>
        /// <returns>A character array representing the specified value.</returns>
        /// <remarks>
        /// Example inputs and encoded outputs:
        /// <list type="table">
        /// <item><term>1</term><description>#1</description></item>
        /// <item><term>10</term><description>#0a</description></item>
        /// <item><term>100</term><description>#64</description></item>
        /// </list>
        /// </remarks>
        internal static char[] HashThenHexValueGenerator(int value)
        {
            return StringToCharArrayWithHashPrefix(value.ToString("X2", CultureInfo.InvariantCulture));
        }

        /// <summary>
        /// Generates a percent prefixed character array representing the specified value in hexadecimal.
        /// </summary>
        /// <param name="value">The source value.</param>
        /// <returns>A character array representing the specified value.</returns>
        /// <remarks>
        /// Example inputs and encoded outputs:
        /// <list type="table">
        /// <item><term>1</term><description>%01</description></item>
        /// <item><term>10</term><description>%0a</description></item>
        /// <item><term>100</term><description>%64</description></item>
        /// </list>
        /// </remarks>
        internal static char[] PercentThenHexValueGenerator(int value)
        {
            return StringToCharArrayWithPercentPrefix(value.ToString("x2", CultureInfo.InvariantCulture));
        }

        /// <summary>
        /// Generates a slash prefixed character array representing the specified value in hexadecimal.
        /// </summary>
        /// <param name="value">The source value.</param>
        /// <returns>A character array representing the specified value.</returns>
        /// <remarks>
        /// Example inputs and encoded outputs:
        /// <list type="table">
        /// <item><term>1</term><description>\01</description></item>
        /// <item><term>10</term><description>\0a</description></item>
        /// <item><term>100</term><description>\64</description></item>
        /// </list>
        /// </remarks>
        internal static char[] SlashThenHexValueGenerator(int value)
        {
            return StringToCharArrayWithSlashPrefix(value.ToString("x2", CultureInfo.InvariantCulture));
        }

        /// <summary>
        /// Generates a slash prefixed character array representing the specified value in hexadecimal.
        /// </summary>
        /// <param name="value">The source value.</param>
        /// <returns>A character array representing the specified value.</returns>
        /// <remarks>
        /// Example inputs and encoded outputs:
        /// <list type="table">
        /// <item><term>1</term><description>\000001</description></item>
        /// <item><term>10</term><description>\000000A</description></item>
        /// <item><term>100</term><description>\000064</description></item>
        /// </list>
        /// </remarks>
        internal static char[] SlashThenSixDigitHexValueGenerator(int value)
        {
            return StringToCharArrayWithSlashPrefix(value.ToString("X6", CultureInfo.InvariantCulture));
        }

        /// <summary>
        /// Generates a hash prefixed character array from the specified string.
        /// </summary>
        /// <param name="value">The source value.</param>
        /// <returns>A character array representing the specified value.</returns>
        /// <remarks>
        /// Example inputs and encoded outputs:
        /// <list type="table">
        /// <item><term>1</term><description>#1</description></item>
        /// <item><term>10</term><description>#10</description></item>
        /// <item><term>100</term><description>#100</description></item>
        /// </list>
        /// </remarks>
        private static char[] StringToCharArrayWithHashPrefix(string value)
        {
            return StringToCharArrayWithPrefix(value, '#');            
        }

        /// <summary>
        /// Generates a percent prefixed character array from the specified string.
        /// </summary>
        /// <param name="value">The source value.</param>
        /// <returns>A character array representing the specified value.</returns>
        /// <remarks>
        /// Example inputs and encoded outputs:
        /// <list type="table">
        /// <item><term>1</term><description>%1</description></item>
        /// <item><term>10</term><description>%10</description></item>
        /// <item><term>100</term><description>%100</description></item>
        /// </list>
        /// </remarks>
        private static char[] StringToCharArrayWithPercentPrefix(string value)
        {
            return StringToCharArrayWithPrefix(value, '%');            
        }

        /// <summary>
        /// Generates a slash prefixed character array from the specified string.
        /// </summary>
        /// <param name="value">The source value.</param>
        /// <returns>A character array representing the specified value.</returns>
        /// <remarks>
        /// Example inputs and encoded outputs:
        /// <list type="table">
        /// <item><term>1</term><description>\1</description></item>
        /// <item><term>10</term><description>\10</description></item>
        /// <item><term>100</term><description>\100</description></item>
        /// </list>
        /// </remarks>
        private static char[] StringToCharArrayWithSlashPrefix(string value)
        {
            return StringToCharArrayWithPrefix(value, '\\');
        }

        /// <summary>
        /// Generates a prefixed character array from the specified string and prefix.
        /// </summary>
        /// <param name="value">The source value.</param>
        /// <param name="prefix">The prefix to use.</param>
        /// <returns>A prefixed character array representing the specified value.</returns>
        private static char[] StringToCharArrayWithPrefix(string value, char prefix)
        {
            var valueAsStringLength = value.Length;

            var valueAsCharArray = new char[valueAsStringLength + 1];
            valueAsCharArray[0] = prefix;
            for (var j = 0; j < valueAsStringLength; j++)
            {
                valueAsCharArray[j + 1] = value[j];
            }

            return valueAsCharArray;
        }

        /// <summary>
        /// Punch appropriate holes for the selected code charts.
        /// </summary>
        /// <param name="safeList">The safe list to punch through.</param>
        /// <param name="codeCharts">The code charts to punch.</param>
        private static void PunchCodeCharts(ref char[][] safeList, LowerCodeCharts codeCharts)
        {
            PunchHolesIfNeeded(ref safeList, Lower.IsFlagSet(codeCharts, LowerCodeCharts.BASIC_LATIN), Lower.BasicLatin());
            PunchHolesIfNeeded(ref safeList, Lower.IsFlagSet(codeCharts, LowerCodeCharts.C1_CONTROLS_AND_LATIN1_SUPPLEMENT), Lower.Latin1Supplement());
            PunchHolesIfNeeded(ref safeList, Lower.IsFlagSet(codeCharts, LowerCodeCharts.LATIN_EXTENDED_A), Lower.LatinExtendedA());
            PunchHolesIfNeeded(ref safeList, Lower.IsFlagSet(codeCharts, LowerCodeCharts.LATIN_EXTENDED_B), Lower.LatinExtendedB());
            PunchHolesIfNeeded(ref safeList, Lower.IsFlagSet(codeCharts, LowerCodeCharts.IPA_EXTENSIONS), Lower.IpaExtensions());
            PunchHolesIfNeeded(ref safeList, Lower.IsFlagSet(codeCharts, LowerCodeCharts.SPACING_MODIFIER_LETTERS), Lower.SpacingModifierLetters());
            PunchHolesIfNeeded(ref safeList, Lower.IsFlagSet(codeCharts, LowerCodeCharts.COMBINING_DIACRITICAL_MARKS), Lower.CombiningDiacriticalMarks());
            PunchHolesIfNeeded(ref safeList, Lower.IsFlagSet(codeCharts, LowerCodeCharts.GREEK_AND_COPTIC), Lower.GreekAndCoptic());
            PunchHolesIfNeeded(ref safeList, Lower.IsFlagSet(codeCharts, LowerCodeCharts.CYRILLIC), Lower.Cyrillic());
            PunchHolesIfNeeded(ref safeList, Lower.IsFlagSet(codeCharts, LowerCodeCharts.CYRILLIC_SUPPLEMENT), Lower.CyrillicSupplement());
            PunchHolesIfNeeded(ref safeList, Lower.IsFlagSet(codeCharts, LowerCodeCharts.ARMENIAN), Lower.Armenian());
            PunchHolesIfNeeded(ref safeList, Lower.IsFlagSet(codeCharts, LowerCodeCharts.HEBREW), Lower.Hebrew());
            PunchHolesIfNeeded(ref safeList, Lower.IsFlagSet(codeCharts, LowerCodeCharts.ARABIC), Lower.Arabic());
            PunchHolesIfNeeded(ref safeList, Lower.IsFlagSet(codeCharts, LowerCodeCharts.SYRIAC), Lower.Syriac());
            PunchHolesIfNeeded(ref safeList, Lower.IsFlagSet(codeCharts, LowerCodeCharts.ARABIC_SUPPLEMENT), Lower.ArabicSupplement());
            PunchHolesIfNeeded(ref safeList, Lower.IsFlagSet(codeCharts, LowerCodeCharts.THAANA), Lower.Thaana());
            PunchHolesIfNeeded(ref safeList, Lower.IsFlagSet(codeCharts, LowerCodeCharts.NKO), Lower.Nko());
            PunchHolesIfNeeded(ref safeList, Lower.IsFlagSet(codeCharts, LowerCodeCharts.SAMARITAN), Lower.Samaritan());
            PunchHolesIfNeeded(ref safeList, Lower.IsFlagSet(codeCharts, LowerCodeCharts.DEVANAGARI), Lower.Devanagari());
            PunchHolesIfNeeded(ref safeList, Lower.IsFlagSet(codeCharts, LowerCodeCharts.BENGALI), Lower.Bengali());
            PunchHolesIfNeeded(ref safeList, Lower.IsFlagSet(codeCharts, LowerCodeCharts.GURMUKHI), Lower.Gurmukhi());
            PunchHolesIfNeeded(ref safeList, Lower.IsFlagSet(codeCharts, LowerCodeCharts.GUJARATI), Lower.Gujarati());
            PunchHolesIfNeeded(ref safeList, Lower.IsFlagSet(codeCharts, LowerCodeCharts.ORIYA), Lower.Oriya());
            PunchHolesIfNeeded(ref safeList, Lower.IsFlagSet(codeCharts, LowerCodeCharts.TAMIL), Lower.Tamil());
            PunchHolesIfNeeded(ref safeList, Lower.IsFlagSet(codeCharts, LowerCodeCharts.TELUGU), Lower.Telugu());
            PunchHolesIfNeeded(ref safeList, Lower.IsFlagSet(codeCharts, LowerCodeCharts.KANNADA), Lower.Kannada());
            PunchHolesIfNeeded(ref safeList, Lower.IsFlagSet(codeCharts, LowerCodeCharts.MALAYALAM), Lower.Malayalam());
            PunchHolesIfNeeded(ref safeList, Lower.IsFlagSet(codeCharts, LowerCodeCharts.SINHALA), Lower.Sinhala());
            PunchHolesIfNeeded(ref safeList, Lower.IsFlagSet(codeCharts, LowerCodeCharts.THAI), Lower.Thai());
            PunchHolesIfNeeded(ref safeList, Lower.IsFlagSet(codeCharts, LowerCodeCharts.LAO), Lower.Lao());
            PunchHolesIfNeeded(ref safeList, Lower.IsFlagSet(codeCharts, LowerCodeCharts.TIBETAN), Lower.Tibetan());
        }

        /// <summary>
        /// Punch appropriate holes for the selected code charts.
        /// </summary>
        /// <param name="safeList">The safe list to punch through.</param>
        /// <param name="codeCharts">The code charts to punch.</param>
        private static void PunchCodeCharts(ref char[][] safeList, LowerMidCodeCharts codeCharts)
        {
            PunchHolesIfNeeded(ref safeList, LowerMiddle.IsFlagSet(codeCharts, LowerMidCodeCharts.MYANMAR), LowerMiddle.Myanmar());
            PunchHolesIfNeeded(ref safeList, LowerMiddle.IsFlagSet(codeCharts, LowerMidCodeCharts.GEORGIAN), LowerMiddle.Georgian());
            PunchHolesIfNeeded(ref safeList, LowerMiddle.IsFlagSet(codeCharts, LowerMidCodeCharts.HANGUL_JAMO), LowerMiddle.HangulJamo());
            PunchHolesIfNeeded(ref safeList, LowerMiddle.IsFlagSet(codeCharts, LowerMidCodeCharts.ETHIOPIC), LowerMiddle.Ethiopic());
            PunchHolesIfNeeded(ref safeList, LowerMiddle.IsFlagSet(codeCharts, LowerMidCodeCharts.ETHIOPIC_SUPPLEMENT), LowerMiddle.EthiopicSupplement());
            PunchHolesIfNeeded(ref safeList, LowerMiddle.IsFlagSet(codeCharts, LowerMidCodeCharts.CHEROKEE), LowerMiddle.Cherokee());
            PunchHolesIfNeeded(ref safeList, LowerMiddle.IsFlagSet(codeCharts, LowerMidCodeCharts.UNIFIED_CANADIAN_ABORIGINAL_SYLLABICS), LowerMiddle.UnifiedCanadianAboriginalSyllabics());
            PunchHolesIfNeeded(ref safeList, LowerMiddle.IsFlagSet(codeCharts, LowerMidCodeCharts.OGHAM), LowerMiddle.Ogham());
            PunchHolesIfNeeded(ref safeList, LowerMiddle.IsFlagSet(codeCharts, LowerMidCodeCharts.RUNIC), LowerMiddle.Runic());
            PunchHolesIfNeeded(ref safeList, LowerMiddle.IsFlagSet(codeCharts, LowerMidCodeCharts.TAGALOG), LowerMiddle.Tagalog());
            PunchHolesIfNeeded(ref safeList, LowerMiddle.IsFlagSet(codeCharts, LowerMidCodeCharts.HANUNOO), LowerMiddle.Hanunoo());
            PunchHolesIfNeeded(ref safeList, LowerMiddle.IsFlagSet(codeCharts, LowerMidCodeCharts.BUHID), LowerMiddle.Buhid());
            PunchHolesIfNeeded(ref safeList, LowerMiddle.IsFlagSet(codeCharts, LowerMidCodeCharts.TAGBANWA), LowerMiddle.Tagbanwa());
            PunchHolesIfNeeded(ref safeList, LowerMiddle.IsFlagSet(codeCharts, LowerMidCodeCharts.KHMER), LowerMiddle.Khmer());
            PunchHolesIfNeeded(ref safeList, LowerMiddle.IsFlagSet(codeCharts, LowerMidCodeCharts.MONGOLIAN), LowerMiddle.Mongolian());
            PunchHolesIfNeeded(ref safeList, LowerMiddle.IsFlagSet(codeCharts, LowerMidCodeCharts.UNIFIED_CANADIAN_ABORIGINAL_SYLLABICS_EXTENDED), LowerMiddle.UnifiedCanadianAboriginalSyllabicsExtended());
            PunchHolesIfNeeded(ref safeList, LowerMiddle.IsFlagSet(codeCharts, LowerMidCodeCharts.LIMBU), LowerMiddle.Limbu());
            PunchHolesIfNeeded(ref safeList, LowerMiddle.IsFlagSet(codeCharts, LowerMidCodeCharts.TAI_LE), LowerMiddle.TaiLe());
            PunchHolesIfNeeded(ref safeList, LowerMiddle.IsFlagSet(codeCharts, LowerMidCodeCharts.NEW_TAI_LUE), LowerMiddle.NewTaiLue());
            PunchHolesIfNeeded(ref safeList, LowerMiddle.IsFlagSet(codeCharts, LowerMidCodeCharts.KHMER_SYMBOLS), LowerMiddle.KhmerSymbols());
            PunchHolesIfNeeded(ref safeList, LowerMiddle.IsFlagSet(codeCharts, LowerMidCodeCharts.BUGINESE), LowerMiddle.Buginese());
            PunchHolesIfNeeded(ref safeList, LowerMiddle.IsFlagSet(codeCharts, LowerMidCodeCharts.TAI_THAM), LowerMiddle.TaiTham());
            PunchHolesIfNeeded(ref safeList, LowerMiddle.IsFlagSet(codeCharts, LowerMidCodeCharts.BALINESE), LowerMiddle.Balinese());
            PunchHolesIfNeeded(ref safeList, LowerMiddle.IsFlagSet(codeCharts, LowerMidCodeCharts.SUDANESE), LowerMiddle.Sudanese());
            PunchHolesIfNeeded(ref safeList, LowerMiddle.IsFlagSet(codeCharts, LowerMidCodeCharts.LEPCHA), LowerMiddle.Lepcha());
            PunchHolesIfNeeded(ref safeList, LowerMiddle.IsFlagSet(codeCharts, LowerMidCodeCharts.OL_CHIKI), LowerMiddle.OlChiki());
            PunchHolesIfNeeded(ref safeList, LowerMiddle.IsFlagSet(codeCharts, LowerMidCodeCharts.VEDIC_EXTENSIONS), LowerMiddle.VedicExtensions());
            PunchHolesIfNeeded(ref safeList, LowerMiddle.IsFlagSet(codeCharts, LowerMidCodeCharts.PHONETIC_EXTENSIONS), LowerMiddle.PhoneticExtensions());
            PunchHolesIfNeeded(ref safeList, LowerMiddle.IsFlagSet(codeCharts, LowerMidCodeCharts.PHONETIC_EXTENSIONS_SUPPLEMENT), LowerMiddle.PhoneticExtensionsSupplement());
            PunchHolesIfNeeded(ref safeList, LowerMiddle.IsFlagSet(codeCharts, LowerMidCodeCharts.COMBINING_DIACRITICAL_MARKS_SUPPLEMENT), LowerMiddle.CombiningDiacriticalMarksSupplement());
            PunchHolesIfNeeded(ref safeList, LowerMiddle.IsFlagSet(codeCharts, LowerMidCodeCharts.LATIN_EXTENDED_ADDITIONAL), LowerMiddle.LatinExtendedAdditional());
        }

        /// <summary>
        /// Punch appropriate holes for the selected code charts.
        /// </summary>
        /// <param name="safeList">The safe list to punch through.</param>
        /// <param name="codeCharts">The code charts to punch.</param>
        private static void PunchCodeCharts(ref char[][] safeList, MidCodeCharts codeCharts)
        {
            PunchHolesIfNeeded(ref safeList, Middle.IsFlagSet(codeCharts, MidCodeCharts.GREEK_EXTENDED), Middle.GreekExtended());
            PunchHolesIfNeeded(ref safeList, Middle.IsFlagSet(codeCharts, MidCodeCharts.GENERAL_PUNCTUATION), Middle.GeneralPunctuation());
            PunchHolesIfNeeded(ref safeList, Middle.IsFlagSet(codeCharts, MidCodeCharts.SUPERSCRIPTS_AND_SUBSCRIPTS), Middle.SuperscriptsAndSubscripts());
            PunchHolesIfNeeded(ref safeList, Middle.IsFlagSet(codeCharts, MidCodeCharts.CURRENCY_SYMBOLS), Middle.CurrencySymbols());
            PunchHolesIfNeeded(ref safeList, Middle.IsFlagSet(codeCharts, MidCodeCharts.COMBINING_DIACRITICAL_MARKS_FOR_SYMBOLS), Middle.CombiningDiacriticalMarksForSymbols());
            PunchHolesIfNeeded(ref safeList, Middle.IsFlagSet(codeCharts, MidCodeCharts.LETTERLIKE_SYMBOLS), Middle.LetterlikeSymbols());
            PunchHolesIfNeeded(ref safeList, Middle.IsFlagSet(codeCharts, MidCodeCharts.NUMBER_FORMS), Middle.NumberForms());
            PunchHolesIfNeeded(ref safeList, Middle.IsFlagSet(codeCharts, MidCodeCharts.ARROWS), Middle.Arrows());
            PunchHolesIfNeeded(ref safeList, Middle.IsFlagSet(codeCharts, MidCodeCharts.MATHEMATICAL_OPERATORS), Middle.MathematicalOperators());
            PunchHolesIfNeeded(ref safeList, Middle.IsFlagSet(codeCharts, MidCodeCharts.MISCELLANEOUS_TECHNICAL), Middle.MiscellaneousTechnical());
            PunchHolesIfNeeded(ref safeList, Middle.IsFlagSet(codeCharts, MidCodeCharts.CONTROL_PICTURES), Middle.ControlPictures());
            PunchHolesIfNeeded(ref safeList, Middle.IsFlagSet(codeCharts, MidCodeCharts.OPTICAL_CHARACTER_RECOGNITION), Middle.OpticalCharacterRecognition());
            PunchHolesIfNeeded(ref safeList, Middle.IsFlagSet(codeCharts, MidCodeCharts.ENCLOSED_ALPHANUMERICS), Middle.EnclosedAlphanumerics());
            PunchHolesIfNeeded(ref safeList, Middle.IsFlagSet(codeCharts, MidCodeCharts.BOX_DRAWING), Middle.BoxDrawing());
            PunchHolesIfNeeded(ref safeList, Middle.IsFlagSet(codeCharts, MidCodeCharts.BLOCK_ELEMENTS), Middle.BlockElements());
            PunchHolesIfNeeded(ref safeList, Middle.IsFlagSet(codeCharts, MidCodeCharts.GEOMETRIC_SHAPES), Middle.GeometricShapes());
            PunchHolesIfNeeded(ref safeList, Middle.IsFlagSet(codeCharts, MidCodeCharts.MISCELLANEOUS_SYMBOLS), Middle.MiscellaneousSymbols());
            PunchHolesIfNeeded(ref safeList, Middle.IsFlagSet(codeCharts, MidCodeCharts.DINGBATS), Middle.Dingbats());
            PunchHolesIfNeeded(ref safeList, Middle.IsFlagSet(codeCharts, MidCodeCharts.MISCELLANEOUS_MATHEMATICAL_SYMBOLS_A), Middle.MiscellaneousMathematicalSymbolsA());
            PunchHolesIfNeeded(ref safeList, Middle.IsFlagSet(codeCharts, MidCodeCharts.SUPPLEMENTAL_ARROWS_A), Middle.SupplementalArrowsA());
            PunchHolesIfNeeded(ref safeList, Middle.IsFlagSet(codeCharts, MidCodeCharts.BRAILLE_PATTERNS), Middle.BraillePatterns());
            PunchHolesIfNeeded(ref safeList, Middle.IsFlagSet(codeCharts, MidCodeCharts.SUPPLEMENTAL_ARROWS_B), Middle.SupplementalArrowsB());
            PunchHolesIfNeeded(ref safeList, Middle.IsFlagSet(codeCharts, MidCodeCharts.MISCELLANEOUS_MATHEMATICAL_SYMBOLS_B), Middle.MiscellaneousMathematicalSymbolsB());
            PunchHolesIfNeeded(ref safeList, Middle.IsFlagSet(codeCharts, MidCodeCharts.SUPPLEMENTAL_MATHEMATICAL_OPERATORS), Middle.SupplementalMathematicalOperators());
            PunchHolesIfNeeded(ref safeList, Middle.IsFlagSet(codeCharts, MidCodeCharts.MISCELLANEOUS_SYMBOLS_AND_ARROWS), Middle.MiscellaneousSymbolsAndArrows());
            PunchHolesIfNeeded(ref safeList, Middle.IsFlagSet(codeCharts, MidCodeCharts.GLAGOLITIC), Middle.Glagolitic());
            PunchHolesIfNeeded(ref safeList, Middle.IsFlagSet(codeCharts, MidCodeCharts.LATIN_EXTENDED_C), Middle.LatinExtendedC());
            PunchHolesIfNeeded(ref safeList, Middle.IsFlagSet(codeCharts, MidCodeCharts.COPTIC), Middle.Coptic());
            PunchHolesIfNeeded(ref safeList, Middle.IsFlagSet(codeCharts, MidCodeCharts.GEORGIAN_SUPPLEMENT), Middle.GeorgianSupplement());
            PunchHolesIfNeeded(ref safeList, Middle.IsFlagSet(codeCharts, MidCodeCharts.TIFINAGH), Middle.Tifinagh());
            PunchHolesIfNeeded(ref safeList, Middle.IsFlagSet(codeCharts, MidCodeCharts.ETHIOPIC_EXTENDED), Middle.EthiopicExtended());
        }

        /// <summary>
        /// Punch appropriate holes for the selected code charts.
        /// </summary>
        /// <param name="safeList">The safe list to punch through.</param>
        /// <param name="codeCharts">The code charts to punch.</param>
        private static void PunchCodeCharts(ref char[][] safeList, UpperMidCodeCharts codeCharts)
        {
            PunchHolesIfNeeded(ref safeList, UpperMiddle.IsFlagSet(codeCharts, UpperMidCodeCharts.CYRILLIC_EXTENDED_A), UpperMiddle.CyrillicExtendedA());
            PunchHolesIfNeeded(ref safeList, UpperMiddle.IsFlagSet(codeCharts, UpperMidCodeCharts.SUPPLEMENTAL_PUNCTUATION), UpperMiddle.SupplementalPunctuation());
            PunchHolesIfNeeded(ref safeList, UpperMiddle.IsFlagSet(codeCharts, UpperMidCodeCharts.CJK_RADICALS_SUPPLEMENT), UpperMiddle.CjkRadicalsSupplement());
            PunchHolesIfNeeded(ref safeList, UpperMiddle.IsFlagSet(codeCharts, UpperMidCodeCharts.KANGXI_RADICALS), UpperMiddle.KangxiRadicals());
            PunchHolesIfNeeded(ref safeList, UpperMiddle.IsFlagSet(codeCharts, UpperMidCodeCharts.IDEOGRAPHIC_DESCRIPTION_CHARACTERS), UpperMiddle.IdeographicDescriptionCharacters());
            PunchHolesIfNeeded(ref safeList, UpperMiddle.IsFlagSet(codeCharts, UpperMidCodeCharts.CJK_SYMBOLS_AND_PUNCTUATION), UpperMiddle.CjkSymbolsAndPunctuation());
            PunchHolesIfNeeded(ref safeList, UpperMiddle.IsFlagSet(codeCharts, UpperMidCodeCharts.HIRAGANA), UpperMiddle.Hiragana());
            PunchHolesIfNeeded(ref safeList, UpperMiddle.IsFlagSet(codeCharts, UpperMidCodeCharts.KATAKANA), UpperMiddle.Katakana());
            PunchHolesIfNeeded(ref safeList, UpperMiddle.IsFlagSet(codeCharts, UpperMidCodeCharts.BOPOMOFO), UpperMiddle.Bopomofo());
            PunchHolesIfNeeded(ref safeList, UpperMiddle.IsFlagSet(codeCharts, UpperMidCodeCharts.HANGUL_COMPATIBILITY_JAMO), UpperMiddle.HangulCompatibilityJamo());
            PunchHolesIfNeeded(ref safeList, UpperMiddle.IsFlagSet(codeCharts, UpperMidCodeCharts.KANBUN), UpperMiddle.Kanbun());
            PunchHolesIfNeeded(ref safeList, UpperMiddle.IsFlagSet(codeCharts, UpperMidCodeCharts.BOPOMOFO_EXTENDED), UpperMiddle.BopomofoExtended());
            PunchHolesIfNeeded(ref safeList, UpperMiddle.IsFlagSet(codeCharts, UpperMidCodeCharts.CJK_STROKES), UpperMiddle.CjkStrokes());
            PunchHolesIfNeeded(ref safeList, UpperMiddle.IsFlagSet(codeCharts, UpperMidCodeCharts.KATAKANA_PHONETIC_EXTENSIONS), UpperMiddle.KatakanaPhoneticExtensions());
            PunchHolesIfNeeded(ref safeList, UpperMiddle.IsFlagSet(codeCharts, UpperMidCodeCharts.ENCLOSED_CJK_LETTERS_AND_MONTHS), UpperMiddle.EnclosedCjkLettersAndMonths());
            PunchHolesIfNeeded(ref safeList, UpperMiddle.IsFlagSet(codeCharts, UpperMidCodeCharts.CJK_COMPATIBILITY), UpperMiddle.CjkCompatibility());
            PunchHolesIfNeeded(ref safeList, UpperMiddle.IsFlagSet(codeCharts, UpperMidCodeCharts.CJK_UNIFIED_IDEOGRAPHS_EXTENSION_A), UpperMiddle.CjkUnifiedIdeographsExtensionA());
            PunchHolesIfNeeded(ref safeList, UpperMiddle.IsFlagSet(codeCharts, UpperMidCodeCharts.YIJING_HEXAGRAM_SYMBOLS), UpperMiddle.YijingHexagramSymbols());
            PunchHolesIfNeeded(ref safeList, UpperMiddle.IsFlagSet(codeCharts, UpperMidCodeCharts.CJK_UNIFIED_IDEOGRAPHS), UpperMiddle.CjkUnifiedIdeographs());
            PunchHolesIfNeeded(ref safeList, UpperMiddle.IsFlagSet(codeCharts, UpperMidCodeCharts.YI_SYLLABLES), UpperMiddle.YiSyllables());
            PunchHolesIfNeeded(ref safeList, UpperMiddle.IsFlagSet(codeCharts, UpperMidCodeCharts.YI_RADICALS), UpperMiddle.YiRadicals());
            PunchHolesIfNeeded(ref safeList, UpperMiddle.IsFlagSet(codeCharts, UpperMidCodeCharts.LISU), UpperMiddle.Lisu());
            PunchHolesIfNeeded(ref safeList, UpperMiddle.IsFlagSet(codeCharts, UpperMidCodeCharts.VAI), UpperMiddle.Vai());
            PunchHolesIfNeeded(ref safeList, UpperMiddle.IsFlagSet(codeCharts, UpperMidCodeCharts.CYRILLIC_EXTENDED_B), UpperMiddle.CyrillicExtendedB());
            PunchHolesIfNeeded(ref safeList, UpperMiddle.IsFlagSet(codeCharts, UpperMidCodeCharts.BAMUM), UpperMiddle.Bamum());
            PunchHolesIfNeeded(ref safeList, UpperMiddle.IsFlagSet(codeCharts, UpperMidCodeCharts.MODIFIER_TONE_LETTERS), UpperMiddle.ModifierToneLetters());
            PunchHolesIfNeeded(ref safeList, UpperMiddle.IsFlagSet(codeCharts, UpperMidCodeCharts.LATIN_EXTENDED_D), UpperMiddle.LatinExtendedD());
            PunchHolesIfNeeded(ref safeList, UpperMiddle.IsFlagSet(codeCharts, UpperMidCodeCharts.SYLOTI_NAGRI), UpperMiddle.SylotiNagri());
            PunchHolesIfNeeded(ref safeList, UpperMiddle.IsFlagSet(codeCharts, UpperMidCodeCharts.COMMON_INDIC_NUMBER_FORMS), UpperMiddle.CommonIndicNumberForms());
            PunchHolesIfNeeded(ref safeList, UpperMiddle.IsFlagSet(codeCharts, UpperMidCodeCharts.PHAGSPA), UpperMiddle.Phagspa());
            PunchHolesIfNeeded(ref safeList, UpperMiddle.IsFlagSet(codeCharts, UpperMidCodeCharts.SAURASHTRA), UpperMiddle.Saurashtra());
        }

        /// <summary>
        /// Punch appropriate holes for the selected code charts.
        /// </summary>
        /// <param name="safeList">The safe list to punch through.</param>
        /// <param name="codeCharts">The code charts to punch.</param>
        private static void PunchCodeCharts(ref char[][] safeList, UpperCodeCharts codeCharts)
        {
            PunchHolesIfNeeded(ref safeList, Upper.IsFlagSet(codeCharts, UpperCodeCharts.DEVANAGARI_EXTENDED), Upper.DevanagariExtended());
            PunchHolesIfNeeded(ref safeList, Upper.IsFlagSet(codeCharts, UpperCodeCharts.KAYAH_LI), Upper.KayahLi());
            PunchHolesIfNeeded(ref safeList, Upper.IsFlagSet(codeCharts, UpperCodeCharts.REJANG), Upper.Rejang());
            PunchHolesIfNeeded(ref safeList, Upper.IsFlagSet(codeCharts, UpperCodeCharts.HANGUL_JAMO_EXTENDED_A), Upper.HangulJamoExtendedA());
            PunchHolesIfNeeded(ref safeList, Upper.IsFlagSet(codeCharts, UpperCodeCharts.JAVANESE), Upper.Javanese());
            PunchHolesIfNeeded(ref safeList, Upper.IsFlagSet(codeCharts, UpperCodeCharts.CHAM), Upper.Cham());
            PunchHolesIfNeeded(ref safeList, Upper.IsFlagSet(codeCharts, UpperCodeCharts.MYANMAR_EXTENDED_A), Upper.MyanmarExtendedA());
            PunchHolesIfNeeded(ref safeList, Upper.IsFlagSet(codeCharts, UpperCodeCharts.TAI_VIET), Upper.TaiViet());
            PunchHolesIfNeeded(ref safeList, Upper.IsFlagSet(codeCharts, UpperCodeCharts.MEETEI_MAYEK), Upper.MeeteiMayek());
            PunchHolesIfNeeded(ref safeList, Upper.IsFlagSet(codeCharts, UpperCodeCharts.HANGUL_SYLLABLES), Upper.HangulSyllables());
            PunchHolesIfNeeded(ref safeList, Upper.IsFlagSet(codeCharts, UpperCodeCharts.HANGUL_JAMO_EXTENDED_B), Upper.HangulJamoExtendedB());
            PunchHolesIfNeeded(ref safeList, Upper.IsFlagSet(codeCharts, UpperCodeCharts.CJK_COMPATIBILITY_IDEOGRAPHS), Upper.CjkCompatibilityIdeographs());
            PunchHolesIfNeeded(ref safeList, Upper.IsFlagSet(codeCharts, UpperCodeCharts.ALPHABETIC_PRESENTATION_FORMS), Upper.AlphabeticPresentationForms());
            PunchHolesIfNeeded(ref safeList, Upper.IsFlagSet(codeCharts, UpperCodeCharts.ARABIC_PRESENTATION_FORMS_A), Upper.ArabicPresentationFormsA());
            PunchHolesIfNeeded(ref safeList, Upper.IsFlagSet(codeCharts, UpperCodeCharts.VARIATION_SELECTORS), Upper.VariationSelectors());
            PunchHolesIfNeeded(ref safeList, Upper.IsFlagSet(codeCharts, UpperCodeCharts.VERTICAL_FORMS), Upper.VerticalForms());
            PunchHolesIfNeeded(ref safeList, Upper.IsFlagSet(codeCharts, UpperCodeCharts.COMBINING_HALF_MARKS), Upper.CombiningHalfMarks());
            PunchHolesIfNeeded(ref safeList, Upper.IsFlagSet(codeCharts, UpperCodeCharts.CJK_COMPATIBILITY_FORMS), Upper.CjkCompatibilityForms());
            PunchHolesIfNeeded(ref safeList, Upper.IsFlagSet(codeCharts, UpperCodeCharts.SMALL_FORM_VARIANTS), Upper.SmallFormVariants());
            PunchHolesIfNeeded(ref safeList, Upper.IsFlagSet(codeCharts, UpperCodeCharts.ARABIC_PRESENTATION_FORMS_B), Upper.ArabicPresentationFormsB());
            PunchHolesIfNeeded(ref safeList, Upper.IsFlagSet(codeCharts, UpperCodeCharts.HALF_WIDTH_AND_FULL_WIDTH_FORMS), Upper.HalfWidthAndFullWidthForms());
            PunchHolesIfNeeded(ref safeList, Upper.IsFlagSet(codeCharts, UpperCodeCharts.SPECIALS), Upper.Specials());
        }

        /// <summary>
        /// Punches holes as necessary.
        /// </summary>
        /// <param name="safeList">The safe list to punch through.</param>
        /// <param name="needed">Value indicating whether the holes should be punched.</param>
        /// <param name="whiteListedCharacters">The list of character positions to punch.</param>
        private static void PunchHolesIfNeeded(ref char[][] safeList, bool needed, IEnumerable whiteListedCharacters)
        {
            if (!needed)
            {
                return;
            }

            foreach (int offset in whiteListedCharacters)
            {
                safeList[offset] = null;
            }
        }
    }
}