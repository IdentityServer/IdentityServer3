// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CodeCharts.cs" company="Microsoft Corporation">
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
//   Enumerations for the various printable code tables within the Unicode UTF space.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

// ReSharper disable CheckNamespace

using System;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.Security.Application
    // ReSharper restore CheckNamespace
{
    /// <summary>
    /// Values for the lowest section of the UTF8 Unicode code tables, from U0000 to U0FFF.
    /// </summary>
    [Flags]
    internal enum LowerCodeCharts : long
    {
        /// <summary>
        /// No code charts from the lower region of the Unicode tables are safe-listed.
        /// </summary>
        NONE                                        = 0,

        /// <summary>
        /// The Basic Latin code table.
        /// </summary>
        /// <remarks>http://www.unicode.org/charts/PDF/U0000.pdf</remarks>
        BASIC_LATIN                                  = 1 << 0x00,
        
        /// <summary>
        /// The C1 Controls and Latin-1 Supplement code table.
        /// </summary>
        /// <remarks>http://www.unicode.org/charts/PDF/U0080.pdf</remarks>
        C1_CONTROLS_AND_LATIN1_SUPPLEMENT               = 1 << 0x01,

        /// <summary>
        /// The Latin Extended-A code table.
        /// </summary>
        /// <remarks>http://www.unicode.org/charts/PDF/U0100.pdf</remarks>
        LATIN_EXTENDED_A                              = 1 << 0x02,
        
        /// <summary>
        /// The Latin Extended-B code table.
        /// </summary>
        /// <remarks>http://www.unicode.org/charts/PDF/U0180.pdf</remarks>
        LATIN_EXTENDED_B                              = 1 << 0x03,
        
        /// <summary>
        /// The IPA Extensions code table.
        /// </summary>
        /// <remarks>http://www.unicode.org/charts/PDF/U0250.pdf</remarks>
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Ipa", Justification = "Unicode Standard")]
        IPA_EXTENSIONS                               = 1 << 0x04,
        
        /// <summary>
        /// The Spacing Modifier Letters code table.
        /// </summary>
        /// <remarks>http://www.unicode.org/charts/PDF/U02B0.pdf</remarks>
        SPACING_MODIFIER_LETTERS                      = 1 << 0x05,

        /// <summary>
        /// The Combining Diacritical Marks code table.
        /// </summary>
        /// <remarks>http://www.unicode.org/charts/PDF/U0300.pdf</remarks>
        COMBINING_DIACRITICAL_MARKS                   = 1 << 0x06,        

        /// <summary>
        /// The Greek and Coptic code table.
        /// </summary>
        /// <remarks>http://www.unicode.org/charts/PDF/U0370.pdf</remarks>
        GREEK_AND_COPTIC                              = 1 << 0x07,
        
        /// <summary>
        /// The Cyrillic code table.
        /// </summary>
        /// <remarks>http://www.unicode.org/charts/PDF/U0400.pdf</remarks>
        CYRILLIC                                    = 1 << 0x08,

        /// <summary>
        /// The Cyrillic Supplement code table.
        /// </summary>
        /// <remarks>http://www.unicode.org/charts/PDF/U0500.pdf</remarks>
        CYRILLIC_SUPPLEMENT                          = 1 << 0x09,
        
        /// <summary>
        /// The Armenian code table.
        /// </summary>
        /// <remarks>http://www.unicode.org/charts/PDF/U0530.pdf</remarks>
        ARMENIAN                                    = 1 << 0x0A,

        /// <summary>
        /// The Hebrew code table.
        /// </summary>
        /// <remarks>http://www.unicode.org/charts/PDF/U0590.pdf</remarks>
        HEBREW                                      = 1 << 0x0B,
        
        /// <summary>
        /// The Arabic code table.
        /// </summary>
        /// <remarks>http://www.unicode.org/charts/PDF/U0600.pdf</remarks>
        ARABIC                                      = 1 << 0x0C,
                
        /// <summary>
        /// The Syriac code table.
        /// </summary>
        /// <remarks>http://www.unicode.org/charts/PDF/U0700.pdf</remarks>
        SYRIAC                                      = 1 << 0x0D,
        
        /// <summary>
        /// The Arabic Supplement code table.
        /// </summary>
        /// <remarks>http://www.unicode.org/charts/PDF/U0750.pdf</remarks>
        ARABIC_SUPPLEMENT                            = 1 << 0x0E,
        
        /// <summary>
        /// The Thaana code table.
        /// </summary>
        /// <remarks>http://www.unicode.org/charts/PDF/U0780.pdf</remarks>
        THAANA                                      = 1 << 0x0F,

        /// <summary>
        /// The Nko code table.
        /// </summary>
        /// <remarks>http://www.unicode.org/charts/PDF/U07C0.pdf</remarks>
        NKO                                         = 1 << 0x10,
        
        /// <summary>
        /// The Samaritan code table.
        /// </summary>
        /// <remarks>http://www.unicode.org/charts/PDF/U0800.pdf</remarks>
        SAMARITAN                                   = 1 << 0x11,
        
        /// <summary>
        /// The Devanagari code table.
        /// </summary>
        /// <remarks>http://www.unicode.org/charts/PDF/U0900.pdf</remarks>
        DEVANAGARI                                  = 1 << 0x12,
        
        /// <summary>
        /// The Bengali code table.
        /// </summary>
        /// <remarks>http://www.unicode.org/charts/PDF/U0980.pdf</remarks>
        BENGALI                                     = 1 << 0x13,
        
        /// <summary>
        /// The Gurmukhi code table.
        /// </summary>
        /// <remarks>http://www.unicode.org/charts/PDF/U0A00.pdf</remarks>
        GURMUKHI                                    = 1 << 0x14,
        
        /// <summary>
        /// The Gujarati code table.
        /// </summary>
        /// <remarks>http://www.unicode.org/charts/PDF/U0A80.pdf</remarks>
        GUJARATI                                    = 1 << 0x15,

        /// <summary>
        /// The Oriya code table.
        /// </summary>
        /// <remarks>http://www.unicode.org/charts/PDF/U0B00.pdf</remarks>
        ORIYA                                       = 1 << 0x16,
        
        /// <summary>
        /// The Tamil code table.
        /// </summary>
        /// <remarks>http://www.unicode.org/charts/PDF/U0B80.pdf</remarks>
        TAMIL                                       = 1 << 0x17,

        /// <summary>
        /// The Telugu code table.
        /// </summary>
        /// <remarks>http://www.unicode.org/charts/PDF/U0C00.pdf</remarks>
        TELUGU                                      = 1 << 0x18,

        /// <summary>
        /// The Kannada code table.
        /// </summary>
        /// <remarks>http://www.unicode.org/charts/PDF/U0C80.pdf</remarks>
        KANNADA                                     = 1 << 0x19,

        /// <summary>
        /// The Malayalam code table.
        /// </summary>
        /// <remarks>http://www.unicode.org/charts/PDF/U0D00.pdf</remarks>
        MALAYALAM                                   = 1 << 0x1A,

        /// <summary>
        /// The Sinhala code table.
        /// </summary>
        /// <remarks>http://www.unicode.org/charts/PDF/U0D80.pdf</remarks>
        SINHALA                                     = 1 << 0x1B,

        /// <summary>
        /// The Thai code table.
        /// </summary>
        /// <remarks>http://www.unicode.org/charts/PDF/U0E00.pdf</remarks>
        THAI                                        = 1 << 0x1C,

        /// <summary>
        /// The Lao code table.
        /// </summary>
        /// <remarks>http://www.unicode.org/charts/PDF/U0E80.pdf</remarks>
        LAO                                         = 1 << 0x1D,

        /// <summary>
        /// The Tibetan code table.
        /// </summary>
        /// <remarks>http://www.unicode.org/charts/PDF/U0F00.pdf</remarks>
        TIBETAN                                     = 1 << 0x1E,

        /// <summary>
        /// The default code tables marked as safe on initialisation.
        /// </summary>
        DEFAULT = BASIC_LATIN | C1_CONTROLS_AND_LATIN1_SUPPLEMENT | LATIN_EXTENDED_A | LATIN_EXTENDED_B | SPACING_MODIFIER_LETTERS | IPA_EXTENSIONS | COMBINING_DIACRITICAL_MARKS
    }

    /// <summary>
    /// Values for the lower-mid section of the UTF8 Unicode code tables, from U1000 to U1EFF.
    /// </summary>
    [Flags]
    internal enum LowerMidCodeCharts : long
    {
        /// <summary>
        /// No code charts from the lower-mid region of the Unicode tables are safe-listed.
        /// </summary>
        NONE                                        = 0,

        /// <summary>
        /// The Myanmar code table.
        /// </summary>
        /// <remarks>http://www.unicode.org/charts/PDF/U1000.pdf</remarks>
        MYANMAR                                      = 1 << 0x00,

        /// <summary>
        /// The Georgian code table.
        /// </summary>
        /// <remarks>http://www.unicode.org/charts/PDF/U10A0.pdf</remarks>
        GEORGIAN                                    = 1 << 0x01,

        /// <summary>
        /// The Hangul Jamo code table.
        /// </summary>
        /// <remarks>http://www.unicode.org/charts/PDF/U1100.pdf</remarks>
        HANGUL_JAMO                                  = 1 << 0x02,

        /// <summary>
        /// The Ethiopic code table.
        /// </summary>
        /// <remarks>http://www.unicode.org/charts/PDF/U1200.pdf</remarks>
        ETHIOPIC                                    = 1 << 0x03,

        /// <summary>
        /// The Ethiopic supplement code table.
        /// </summary>
        /// <remarks>http://www.unicode.org/charts/PDF/U1380.pdf</remarks>
        ETHIOPIC_SUPPLEMENT                          = 1 << 0x04,

        /// <summary>
        /// The Cherokee code table.
        /// </summary>
        /// <remarks>http://www.unicode.org/charts/PDF/U13A0.pdf</remarks>
        CHEROKEE                                    = 1 << 0x05,

        /// <summary>
        /// The Unified Canadian Aboriginal Syllabics code table.
        /// </summary>
        /// <remarks>http://www.unicode.org/charts/PDF/U1400.pdf</remarks>
        UNIFIED_CANADIAN_ABORIGINAL_SYLLABICS          = 1 << 0x06,
        
        /// <summary>
        /// The Ogham code table.
        /// </summary>
        /// <remarks>http://www.unicode.org/charts/PDF/U1680.pdf</remarks>
        OGHAM                                       = 1 << 0x07,
                        
        /// <summary>
        /// The Runic code table.
        /// </summary>
        /// <remarks>http://www.unicode.org/charts/PDF/U16A0.pdf</remarks>
        RUNIC                                       = 1 << 0x08,

        /// <summary>
        /// The Tagalog code table.
        /// </summary>
        /// <remarks>http://www.unicode.org/charts/PDF/U1700.pdf</remarks>
        TAGALOG                                     = 1 << 0x09,

        /// <summary>
        /// The Hanunoo code table.
        /// </summary>
        /// <remarks>http://www.unicode.org/charts/PDF/U1720.pdf</remarks>
        HANUNOO                                     = 1 << 0x0A,

        /// <summary>
        /// The Buhid code table.
        /// </summary>
        /// <remarks>http://www.unicode.org/charts/PDF/U1740.pdf</remarks>
        BUHID                                       = 1 << 0x0B,

        /// <summary>
        /// The Tagbanwa code table.
        /// </summary>
        /// <remarks>http://www.unicode.org/charts/PDF/U1760.pdf</remarks>
        TAGBANWA                                    = 1 << 0x0C,

        /// <summary>
        /// The Khmer code table.
        /// </summary>
        /// <remarks>http://www.unicode.org/charts/PDF/U1780.pdf</remarks>
        KHMER                                       = 1 << 0x0D,

        /// <summary>
        /// The Mongolian code table.
        /// </summary>
        /// <remarks>http://www.unicode.org/charts/PDF/U1800.pdf</remarks>
        MONGOLIAN                                   = 1 << 0x0E,
                
        /// <summary>
        /// The Unified Canadian Aboriginal Syllabics Extended code table.
        /// </summary>
        /// <remarks>http://www.unicode.org/charts/PDF/U18B0.pdf</remarks>
        UNIFIED_CANADIAN_ABORIGINAL_SYLLABICS_EXTENDED  = 1 << 0x0F,

        /// <summary>
        /// The Limbu code table.
        /// </summary>
        /// <remarks>http://www.unicode.org/charts/PDF/U1900.pdf</remarks>
        LIMBU                                       = 1 << 0x10,
        
        /// <summary>
        /// The Tai Le code table.
        /// </summary>
        /// <remarks>http://www.unicode.org/charts/PDF/U1950.pdf</remarks>
        TAI_LE                                       = 1 << 0x11,

        /// <summary>
        /// The New Tai Lue code table.
        /// </summary>
        /// <remarks>http://www.unicode.org/charts/PDF/U1980.pdf</remarks>
        NEW_TAI_LUE                                   = 1 << 0x12,

        /// <summary>
        /// The Khmer Symbols code table
        /// </summary>
        /// <remarks>http://www.unicode.org/charts/PDF/U19E0.pdf</remarks>
        KHMER_SYMBOLS                                = 1 << 0x13,

        /// <summary>
        /// The Buginese code table.
        /// </summary>
        /// <remarks>http://www.unicode.org/charts/PDF/U1A00.pdf</remarks>
        BUGINESE                                    = 1 << 0x14,
        
        /// <summary>
        /// The Tai Tham code table.
        /// </summary>
        /// <remarks>http://www.unicode.org/charts/PDF/U1A20.pdf</remarks>
        TAI_THAM                                     = 1 << 0x15,

        /// <summary>
        /// The Balinese code table.
        /// </summary>
        /// <remarks>http://www.unicode.org/charts/PDF/U1B00.pdf</remarks>
        BALINESE                                    = 1 << 0x16,

        /// <summary>
        /// The Sudanese code table.
        /// </summary>
        /// <remarks>http://www.unicode.org/charts/PDF/U1B80.pdf</remarks>
        SUDANESE                                    = 1 << 0x17,
        
        /// <summary>
        /// The Lepcha code table.
        /// </summary>
        /// <remarks>http://www.unicode.org/charts/PDF/U1C00.pdf</remarks>
        LEPCHA                                      = 1 << 0x18,
        
        /// <summary>
        /// The Ol Chiki code table.
        /// </summary>
        /// <remarks>http://www.unicode.org/charts/PDF/U1C50.pdf</remarks>
        OL_CHIKI                                     = 1 << 0x19,
        
        /// <summary>
        /// The Vedic Extensions code table.
        /// </summary>
        /// <remarks>http://www.unicode.org/charts/PDF/U1CD0.pdf</remarks>
        VEDIC_EXTENSIONS                             = 1 << 0x1A,
        
        /// <summary>
        /// The Phonetic Extensions code table.
        /// </summary>
        /// <remarks>http://www.unicode.org/charts/PDF/U1D00.pdf</remarks>
        PHONETIC_EXTENSIONS                          = 1 << 0x1B,
        
        /// <summary>
        /// The Phonetic Extensions Supplement code table.
        /// </summary>
        /// <remarks>http://www.unicode.org/charts/PDF/U1D80.pdf</remarks>
        PHONETIC_EXTENSIONS_SUPPLEMENT                = 1 << 0x1C,        
        
        /// <summary>
        /// The Combining Diacritical Marks Supplement code table.
        /// </summary>
        /// <remarks>http://www.unicode.org/charts/PDF/U1DC0.pdf</remarks>        
        COMBINING_DIACRITICAL_MARKS_SUPPLEMENT         = 1 << 0x1D,

        /// <summary>
        /// The Latin Extended Additional code table.
        /// </summary>
        /// <remarks>http://www.unicode.org/charts/PDF/U1E00.pdf</remarks>
        LATIN_EXTENDED_ADDITIONAL                     = 1 << 0x1E
    }

    /// <summary>
    /// Values for the middle section of the UTF8 Unicode code tables, from U1F00 to U2DDF
    /// </summary>
    [Flags]
    internal enum MidCodeCharts : long
    {
        /// <summary>
        /// No code charts from the lower region of the Unicode tables are safe-listed.
        /// </summary>
        NONE                                        = 0,
        
        /// <summary>
        /// The Greek Extended code table.
        /// </summary>
        /// <remarks>http://www.unicode.org/charts/PDF/U1F00.pdf</remarks>
        GREEK_EXTENDED                               = 1 << 0x00,

        /// <summary>
        /// The General Punctuation code table.
        /// </summary>
        /// <remarks>http://www.unicode.org/charts/PDF/U2000.pdf</remarks>
        GENERAL_PUNCTUATION                          = 1 << 0x01,
        
        /// <summary>
        /// The Superscripts and Subscripts code table.
        /// </summary>
        /// <remarks>http://www.unicode.org/charts/PDF/U2070.pdf</remarks>
        SUPERSCRIPTS_AND_SUBSCRIPTS                   = 1 << 0x02,
        
        /// <summary>
        /// The Currency Symbols code table.
        /// </summary>
        /// <remarks>http://www.unicode.org/charts/PDF/U20A0.pdf</remarks>
        CURRENCY_SYMBOLS                             = 1 << 0x03,

        /// <summary>
        /// The Combining Diacritical Marks for Symbols code table.
        /// </summary>
        /// <remarks>http://www.unicode.org/charts/PDF/U20D0.pdf</remarks>
        COMBINING_DIACRITICAL_MARKS_FOR_SYMBOLS         = 1 << 0x04,

        /// <summary>
        /// The Letterlike Symbols code table.
        /// </summary>
        /// <remarks>http://www.unicode.org/charts/PDF/U2100.pdf</remarks>
        LETTERLIKE_SYMBOLS                           = 1 << 0x05,

        /// <summary>
        /// The Number Forms code table.
        /// </summary>
        /// <remarks>http://www.unicode.org/charts/PDF/U2150.pdf</remarks>
        NUMBER_FORMS                                 = 1 << 0x06,

        /// <summary>
        /// The Arrows code table.
        /// </summary>
        /// <remarks>http://www.unicode.org/charts/PDF/U2190.pdf</remarks>
        ARROWS                                      = 1 << 0x07,

        /// <summary>
        /// The Mathematical Operators code table.
        /// </summary>
        /// <remarks>http://www.unicode.org/charts/PDF/U2200.pdf</remarks>
        MATHEMATICAL_OPERATORS                       = 1 << 0x08,
        
        /// <summary>
        /// The Miscellaneous Technical code table.
        /// </summary>
        /// <remarks>http://www.unicode.org/charts/PDF/U2300.pdf</remarks>
        MISCELLANEOUS_TECHNICAL                      = 1 << 0x09,

        /// <summary>
        /// The Control Pictures code table.
        /// </summary>
        /// <remarks>http://www.unicode.org/charts/PDF/U2400.pdf</remarks>
        CONTROL_PICTURES                             = 1 << 0x0A,

        /// <summary>
        /// The Optical Character Recognition table.
        /// </summary>
        /// <remarks>http://www.unicode.org/charts/PDF/U2440.pdf</remarks>
        OPTICAL_CHARACTER_RECOGNITION                 = 1 << 0x0B,

        /// <summary>
        /// The Enclosed Alphanumeric code table.
        /// </summary>
        /// <remarks>http://www.unicode.org/charts/PDF/U2460.pdf</remarks>
        ENCLOSED_ALPHANUMERICS                       = 1 << 0x0C,

        /// <summary>
        /// The Box Drawing code table.
        /// </summary>
        /// <remarks>http://www.unicode.org/charts/PDF/U2500.pdf</remarks>
        BOX_DRAWING                                  = 1 << 0x0D,

        /// <summary>
        /// The Block Elements code table.
        /// </summary>
        /// <remarks>http://www.unicode.org/charts/PDF/U2580.pdf</remarks>
        BLOCK_ELEMENTS                               = 1 << 0x0E,

        /// <summary>
        /// The Geometric Shapes code table.
        /// </summary>
        /// <remarks>http://www.unicode.org/charts/PDF/U25A0.pdf</remarks>
        GEOMETRIC_SHAPES                             = 1 << 0x0F,

        /// <summary>
        /// The Miscellaneous Symbols code table.
        /// </summary>
        /// <remarks>http://www.unicode.org/charts/PDF/U2600.pdf</remarks>
        MISCELLANEOUS_SYMBOLS                        = 1 << 0x10,

        /// <summary>
        /// The Dingbats code table.
        /// </summary>
        /// <remarks>http://www.unicode.org/charts/PDF/U2700.pdf</remarks>
        DINGBATS                                    = 1 << 0x11,
        
        /// <summary>
        /// The Miscellaneous Mathematical Symbols-A code table.
        /// </summary>
        /// <remarks>http://www.unicode.org/charts/PDF/U27C0.pdf</remarks>
        MISCELLANEOUS_MATHEMATICAL_SYMBOLS_A           = 1 << 0x12,
        
        /// <summary>
        /// The Supplemental Arrows-A code table.
        /// </summary>
        /// <remarks>http://www.unicode.org/charts/PDF/U27F0.pdf</remarks>
        SUPPLEMENTAL_ARROWS_A                         = 1 << 0x13,

        /// <summary>
        /// The Braille Patterns code table.
        /// </summary>
        /// <remarks>http://www.unicode.org/charts/PDF/U2800.pdf</remarks>
        BRAILLE_PATTERNS                             = 1 << 0x14,
        
        /// <summary>
        /// The Supplemental Arrows-B code table.
        /// </summary>
        /// <remarks>http://www.unicode.org/charts/PDF/U2900.pdf</remarks>
        SUPPLEMENTAL_ARROWS_B                         = 1 << 0x15,

        /// <summary>
        /// The Miscellaneous Mathematical Symbols-B code table.
        /// </summary>
        /// <remarks>http://www.unicode.org/charts/PDF/U2980.pdf</remarks>                
        MISCELLANEOUS_MATHEMATICAL_SYMBOLS_B           = 1 << 0x16,
                
        /// <summary>
        /// The Supplemental Mathematical Operators code table.
        /// </summary>
        /// <remarks>http://www.unicode.org/charts/PDF/U2A00.pdf</remarks>
        SUPPLEMENTAL_MATHEMATICAL_OPERATORS           = 1 << 0x17,

        /// <summary>
        /// The Miscellaneous Symbols and Arrows code table.
        /// </summary>
        /// <remarks>http://www.unicode.org/charts/PDF/U2B00.pdf</remarks>        
        MISCELLANEOUS_SYMBOLS_AND_ARROWS               = 1 << 0x18,
        
        /// <summary>
        /// The Glagolitic code table.
        /// </summary>
        /// <remarks>http://www.unicode.org/charts/PDF/U2C00.pdf</remarks>
        GLAGOLITIC                                  = 1 << 0x19,
        
        /// <summary>
        /// The Latin Extended-C code table.
        /// </summary>
        /// <remarks>http://www.unicode.org/charts/PDF/U2C60.pdf</remarks>        
        LATIN_EXTENDED_C                              = 1 << 0x1A,
        
        /// <summary>
        /// The Coptic code table.
        /// </summary>
        /// <remarks>http://www.unicode.org/charts/PDF/U2C80.pdf</remarks>
        COPTIC                                      = 1 << 0x1B,
        
        /// <summary>
        /// The Georgian Supplement code table.
        /// </summary>
        /// <remarks>http://www.unicode.org/charts/PDF/U2D00.pdf</remarks>
        GEORGIAN_SUPPLEMENT                          = 1 << 0x1C,

        /// <summary>
        /// The Tifinagh code table.
        /// </summary>
        /// <remarks>http://www.unicode.org/charts/PDF/U2D30.pdf</remarks>
        TIFINAGH                                    = 1 << 0x1D,

        /// <summary>
        /// The Ethiopic Extended code table.
        /// </summary>
        /// <remarks>http://www.unicode.org/charts/PDF/U2D80.pdf</remarks>
        ETHIOPIC_EXTENDED                            = 1 << 0x0E,    
    }

    /// <summary>
    /// Values for the upper middle section of the UTF8 Unicode code tables, from U2DE0 to UA8DF
    /// </summary>
    [Flags]
    internal enum UpperMidCodeCharts : long
    {
        /// <summary>
        /// No code charts from the lower region of the Unicode tables are safe-listed.
        /// </summary>
        NONE                                        = 0,                  
        
        /// <summary>
        /// The Cyrillic Extended-A code table.
        /// </summary>
        /// <remarks>http://www.unicode.org/charts/PDF/U2DE0.pdf</remarks>
        CYRILLIC_EXTENDED_A                           = 1 << 0x00,
        
        /// <summary>
        /// The Supplemental Punctuation code table.
        /// </summary>
        /// <remarks>http://www.unicode.org/charts/PDF/U2E00.pdf</remarks>
        SUPPLEMENTAL_PUNCTUATION                     = 1 << 0x01,
        
        /// <summary>
        /// The CJK Radicials Supplement code table.
        /// </summary>
        /// <remarks>http://www.unicode.org/charts/PDF/U2E80.pdf</remarks>
        CJK_RADICALS_SUPPLEMENT                       = 1 << 0x02,
        
        /// <summary>
        /// The Kangxi Radicials code table.
        /// </summary>
        /// <remarks>http://www.unicode.org/charts/PDF/U2F00.pdf</remarks>
        KANGXI_RADICALS                              = 1 << 0x03,
        
        /// <summary>
        /// The Ideographic Description Characters code table.
        /// </summary>
        /// <remarks>http://www.unicode.org/charts/PDF/U2FF0.pdf</remarks>
        IDEOGRAPHIC_DESCRIPTION_CHARACTERS            = 1 << 0x04,
        
        /// <summary>
        /// The CJK Symbols and Punctuation code table.
        /// </summary>
        /// <remarks>http://www.unicode.org/charts/PDF/U3000.pdf</remarks>
        CJK_SYMBOLS_AND_PUNCTUATION                    = 1 << 0x05,

        /// <summary>
        /// The Hiragana code table.
        /// </summary>
        /// <remarks>http://www.unicode.org/charts/PDF/U3040.pdf</remarks>
        HIRAGANA                                    = 1 << 0x06,
        
        /// <summary>
        /// The Katakana code table.
        /// </summary>
        /// <remarks>http://www.unicode.org/charts/PDF/U30A0.pdf</remarks>
        KATAKANA                                    = 1 << 0x07,

        /// <summary>
        /// The Bopomofo code table.
        /// <seealso cref="BOPOMOFO_EXTENDED"/>
        /// </summary>
        /// <remarks>http://www.unicode.org/charts/PDF/U3100.pdf</remarks>
        BOPOMOFO                                    = 1 << 0x08,

        /// <summary>
        /// The Hangul Compatbility Jamo code table.
        /// </summary>
        /// <remarks>http://www.unicode.org/charts/PDF/U3130.pdf</remarks>
        HANGUL_COMPATIBILITY_JAMO                     = 1 << 0x09,

        /// <summary>
        /// The Kanbun code table.
        /// </summary>
        /// <remarks>http://www.unicode.org/charts/PDF/U3190.pdf</remarks>
        KANBUN                                      = 1 << 0x0A,

        /// <summary>
        /// The Bopomofu Extended code table.
        /// <seealso cref="BOPOMOFO"/>
        /// </summary>
        /// <remarks>http://www.unicode.org/charts/PDF/U31A0.pdf</remarks>
        BOPOMOFO_EXTENDED                            = 1 << 0x0B,

        /// <summary>
        /// The CJK Strokes code table.
        /// </summary>
        /// <remarks>http://www.unicode.org/charts/PDF/U31C0.pdf</remarks>
        CJK_STROKES                                  = 1 << 0x0C,
        
        /// <summary>
        /// The Katakana Phonetic Extensoins code table.
        /// </summary>
        /// <remarks>http://www.unicode.org/charts/PDF/U31F0.pdf</remarks>
        KATAKANA_PHONETIC_EXTENSIONS                  = 1 << 0x0D,

        /// <summary>
        /// The Enclosed CJK Letters and Months code table.
        /// </summary>
        /// <remarks>http://www.unicode.org/charts/PDF/U3200.pdf</remarks>
        ENCLOSED_CJK_LETTERS_AND_MONTHS                 = 1 << 0x0E,

        /// <summary>
        /// The CJK Compatibility code table.
        /// </summary>
        /// <remarks>http://www.unicode.org/charts/PDF/U3300.pdf</remarks>
        CJK_COMPATIBILITY                            = 1 << 0x0F,

        /// <summary>
        /// The CJK Unified Ideographs Extension A code table.
        /// </summary>
        /// <remarks>http://www.unicode.org/charts/PDF/U3400.pdf</remarks>
        CJK_UNIFIED_IDEOGRAPHS_EXTENSION_A              = 1 << 0x10,

        /// <summary>
        /// The Yijing Hexagram Symbols code table.
        /// </summary>
        /// <remarks>http://www.unicode.org/charts/PDF/U4DC0.pdf</remarks>
        YIJING_HEXAGRAM_SYMBOLS                       = 1 << 0x11,
        
        /// <summary>
        /// The CJK Unified Ideographs code table.
        /// </summary>
        /// <remarks>http://www.unicode.org/charts/PDF/U4E00.pdf</remarks>
        CJK_UNIFIED_IDEOGRAPHS                        = 1 << 0x12,
        
        /// <summary>
        /// The Yi Syllables code table.
        /// </summary>
        /// <remarks>http://www.unicode.org/charts/PDF/UA000.pdf</remarks>
        YI_SYLLABLES                                 = 1 << 0x13,

        /// <summary>
        /// The Yi Radicals code table.
        /// </summary>
        /// <remarks>http://www.unicode.org/charts/PDF/UA490.pdf</remarks>
        YI_RADICALS                                  = 1 << 0x14,

        /// <summary>
        /// The Lisu code table.
        /// </summary>
        /// <remarks>http://www.unicode.org/charts/PDF/UA4D0.pdf</remarks>        
        LISU                                        = 1 << 0x15,

        /// <summary>
        /// The Vai code table.
        /// </summary>
        /// <remarks>http://www.unicode.org/charts/PDF/UA500.pdf</remarks>
        VAI                                         = 1 << 0x16,

        /// <summary>
        /// The Cyrillic Extended-B code table.
        /// </summary>
        /// <remarks>http://www.unicode.org/charts/PDF/UA640.pdf</remarks>
        CYRILLIC_EXTENDED_B                           = 1 << 0x17,

        /// <summary>
        /// The Bamum code table.
        /// </summary>
        /// <remarks>http://www.unicode.org/charts/PDF/UA6A0.pdf</remarks>
        BAMUM                                       = 1 << 0x18,

        /// <summary>
        /// The Modifier Tone Letters code table.
        /// </summary>
        /// <remarks>http://www.unicode.org/charts/PDF/UA700.pdf</remarks>
        MODIFIER_TONE_LETTERS                         = 1 << 0x19,

        /// <summary>
        /// The Latin Extended-D code table.
        /// </summary>
        /// <remarks>http://www.unicode.org/charts/PDF/UA720.pdf</remarks>
        LATIN_EXTENDED_D                              = 1 << 0x1A,

        /// <summary>
        /// The Syloti Nagri code table.
        /// </summary>
        /// <remarks>http://www.unicode.org/charts/PDF/UA800.pdf</remarks>
        SYLOTI_NAGRI                                 = 1 << 0x1B,

        /// <summary>
        /// The Common Indic Number Forms code table.
        /// </summary>
        /// <remarks>http://www.unicode.org/charts/PDF/UA830.pdf</remarks>
        COMMON_INDIC_NUMBER_FORMS                      = 1 << 0x1C,

        /// <summary>
        /// The Phags-pa code table.
        /// </summary>
        /// <remarks>http://www.unicode.org/charts/PDF/UA840.pdf</remarks>
        PHAGSPA                                     = 1 << 0x1D,

        /// <summary>
        /// The Saurashtra code table.
        /// </summary>
        /// <remarks>http://www.unicode.org/charts/PDF/UA880.pdf</remarks>
        SAURASHTRA                                  = 1 << 0x1E,
    }

    /// <summary>
    /// Values for the upper section of the UTF8 Unicode code tables, from UA8E0 to UFFFD
    /// </summary>
    [Flags]
    internal enum UpperCodeCharts
    {
        /// <summary>
        /// No code charts from the upper region of the Unicode tables are safe-listed.
        /// </summary>
        NONE                                        = 0,

        /// <summary>
        /// The Devanagari Extended code table.
        /// </summary>
        /// <remarks>http://www.unicode.org/charts/PDF/UA8E0.pdf</remarks>
        DEVANAGARI_EXTENDED                          = 1 << 0x00,

        /// <summary>
        /// The Kayah Li code table.
        /// </summary>
        /// <remarks>http://www.unicode.org/charts/PDF/UA900.pdf</remarks>
        KAYAH_LI                                     = 1 << 0x01,

        /// <summary>
        /// The Rejang code table.
        /// </summary>
        /// <remarks>http://www.unicode.org/charts/PDF/UA930.pdf</remarks>
        REJANG                                      = 1 << 0x02,

        /// <summary>
        /// The Hangul Jamo Extended-A code table.
        /// </summary>
        /// <remarks>http://www.unicode.org/charts/PDF/UA960.pdf</remarks>
        HANGUL_JAMO_EXTENDED_A                         = 1 << 0x03,

        /// <summary>
        /// The Javanese code table.
        /// </summary>
        /// <remarks>http://www.unicode.org/charts/PDF/UA980.pdf</remarks>
        JAVANESE                                    = 1 << 0x04,

        /// <summary>
        /// The Cham code table.
        /// </summary>
        /// <remarks>http://www.unicode.org/charts/PDF/UAA00.pdf</remarks>
        CHAM                                        = 1 << 0x05,

        /// <summary>
        /// The Myanmar Extended-A code table.
        /// </summary>
        /// <remarks>http://www.unicode.org/charts/PDF/UAA60.pdf</remarks>
        MYANMAR_EXTENDED_A                            = 1 << 0x06,

        /// <summary>
        /// The Tai Viet code table.
        /// </summary>
        /// <remarks>http://www.unicode.org/charts/PDF/UAA80.pdf</remarks>
        TAI_VIET                                     = 1 << 0x07,

        /// <summary>
        /// The Meetei Mayek code table.
        /// </summary>
        /// <remarks>http://www.unicode.org/charts/PDF/UABC0.pdf</remarks>
        MEETEI_MAYEK                                 = 1 << 0x08,

        /// <summary>
        /// The Hangul Syllables code table.
        /// </summary>
        /// <remarks>http://www.unicode.org/charts/PDF/UAC00.pdf</remarks>
        HANGUL_SYLLABLES                             = 1 << 0x09,

        /// <summary>
        /// The Hangul Jamo Extended-B code table.
        /// </summary>
        /// <remarks>http://www.unicode.org/charts/PDF/UD7B0.pdf</remarks>
        HANGUL_JAMO_EXTENDED_B                         = 1 << 0x0A,
        
        /// <summary>
        /// The CJK Compatibility Ideographs code table.
        /// </summary>
        /// <remarks>http://www.unicode.org/charts/PDF/UF900.pdf</remarks>
        CJK_COMPATIBILITY_IDEOGRAPHS                  = 1 << 0x0B,
        
        /// <summary>
        /// The Alphabetic Presentation Forms code table.
        /// </summary>
        /// <remarks>http://www.unicode.org/charts/PDF/UFB00.pdf</remarks>
        ALPHABETIC_PRESENTATION_FORMS                 = 1 << 0x0C,
        
        /// <summary>
        /// The Arabic Presentation Forms-A code table.
        /// </summary>
        /// <remarks>http://www.unicode.org/charts/PDF/UFB50.pdf</remarks>
        ARABIC_PRESENTATION_FORMS_A                    = 1 << 0x0D,
        
        /// <summary>
        /// The Variation Selectors code table.
        /// </summary>
        /// <remarks>http://www.unicode.org/charts/PDF/UFE00.pdf</remarks>
        VARIATION_SELECTORS                          = 1 << 0x0E,
        
        /// <summary>
        /// The Vertical Forms code table.
        /// </summary>
        /// <remarks>http://www.unicode.org/charts/PDF/UFE10.pdf</remarks>
        VERTICAL_FORMS                               = 1 << 0x0F,
        
        /// <summary>
        /// The Combining Half Marks code table.
        /// </summary>
        /// <remarks>http://www.unicode.org/charts/PDF/UFE20.pdf</remarks>
        COMBINING_HALF_MARKS                          = 1 << 0x10,
        
        /// <summary>
        /// The CJK Compatibility Forms code table.
        /// </summary>
        /// <remarks>http://www.unicode.org/charts/PDF/UFE30.pdf</remarks>
        CJK_COMPATIBILITY_FORMS                       = 1 << 0x11,
        
        /// <summary>
        /// The Small Form Variants code table.
        /// </summary>
        /// <remarks>http://www.unicode.org/charts/PDF/UFE50.pdf</remarks>
        SMALL_FORM_VARIANTS                           = 1 << 0x12,
        
        /// <summary>
        /// The Arabic Presentation Forms-B code table.
        /// </summary>
        /// <remarks>http://www.unicode.org/charts/PDF/UFE70.pdf</remarks>
        ARABIC_PRESENTATION_FORMS_B                    = 1 << 0x13,
        
        /// <summary>
        /// The half width and full width Forms code table.
        /// </summary>
        /// <remarks>http://www.unicode.org/charts/PDF/UFF00.pdf</remarks>
        HALF_WIDTH_AND_FULL_WIDTH_FORMS                  = 1 << 0x14,
        
        /// <summary>
        /// The Specials code table.
        /// </summary>
        /// <remarks>http://www.unicode.org/charts/PDF/UFFF0.pdf</remarks>
        SPECIALS                                    = 1 << 0x15,
    }
}
