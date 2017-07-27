// ==========================================================================
//  StringExtensions.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Squidex.Infrastructure
{
    public static class StringExtensions
    {
        private static readonly Regex SlugRegex = new Regex("^[a-z0-9]+(\\-[a-z0-9]+)*$", RegexOptions.Compiled);
        private static readonly Regex PropertyNameRegex = new Regex("^[a-zA-Z0-9]+(\\-[a-zA-Z0-9]+)*$", RegexOptions.Compiled);
        private static readonly Dictionary<char, string> LowerCaseDiacritics;
        private static readonly Dictionary<char, string> Diacritics = new Dictionary<char, string>
        {
            ['À'] = "A",
            ['à'] = "a",
            ['Ā'] = "A",
            ['Ġ'] = "G",
            ['ŀ'] = "l",
            ['Š'] = "S",
            ['Ǡ'] = "A",
            ['Ȁ'] = "A",
            ['Á'] = "A",
            ['á'] = "a",
            ['ā'] = "a",
            ['ġ'] = "g",
            ['Ł'] = "L",
            ['š'] = "s",
            ['ǡ'] = "a",
            ['ȁ'] = "a",
            ['Â'] = "A",
            ['â'] = "a",
            ['Ă'] = "A",
            ['Ģ'] = "G",
            ['ł'] = "l",
            ['Ţ'] = "T",
            ['Ǣ'] = "Ae",
            ['Ȃ'] = "A",
            ['Ã'] = "A",
            ['ã'] = "a",
            ['ă'] = "a",
            ['ģ'] = "g",
            ['Ń'] = "N",
            ['ţ'] = "t",
            ['ǣ'] = "ae",
            ['ȃ'] = "a",
            ['Ä'] = "Ae",
            ['ä'] = "ae",
            ['Ą'] = "A",
            ['Ĥ'] = "H",
            ['ń'] = "n",
            ['Ť'] = "T",
            ['Ǆ'] = "DZ",
            ['Ǥ'] = "G",
            ['Ȅ'] = "E",
            ['Å'] = "A",
            ['å'] = "a",
            ['ą'] = "a",
            ['ĥ'] = "h",
            ['Ņ'] = "N",
            ['ť'] = "t",
            ['ǅ'] = "Dz",
            ['ǥ'] = "g",
            ['ȅ'] = "e",
            ['Æ'] = "AE",
            ['æ'] = "ae",
            ['Ć'] = "C",
            ['Ħ'] = "H",
            ['ņ'] = "n",
            ['Ŧ'] = "T",
            ['ǆ'] = "dz",
            ['Ǧ'] = "G",
            ['Ȇ'] = "E",
            ['Ç'] = "C",
            ['ç'] = "c",
            ['ć'] = "c",
            ['ħ'] = "h",
            ['Ň'] = "N",
            ['ŧ'] = "t",
            ['Ǉ'] = "W",
            ['ǧ'] = "g",
            ['ȇ'] = "e",
            ['È'] = "E",
            ['è'] = "E",
            ['Ĉ'] = "C",
            ['Ĩ'] = "I",
            ['ň'] = "n",
            ['Ũ'] = "U",
            ['ǈ'] = "Lj",
            ['Ǩ'] = "K",
            ['Ȉ'] = "I",
            ['É'] = "E",
            ['é'] = "e",
            ['ĉ'] = "c",
            ['ĩ'] = "i",
            ['ŉ'] = "n",
            ['ũ'] = "u",
            ['ǉ'] = "lj",
            ['ǩ'] = "k",
            ['ȉ'] = "i",
            ['Ê'] = "E",
            ['ê'] = "e",
            ['Ċ'] = "C",
            ['Ī'] = "I",
            ['Ŋ'] = "n",
            ['Ū'] = "U",
            ['Ǌ'] = "NJ",
            ['Ǫ'] = "O",
            ['Ȋ'] = "I",
            ['Ë'] = "E",
            ['ë'] = "e",
            ['ċ'] = "c",
            ['ī'] = "i",
            ['ŋ'] = "n",
            ['ū'] = "u",
            ['ǋ'] = "Nj",
            ['ǫ'] = "o",
            ['ȋ'] = "i",
            ['Ì'] = "I",
            ['ì'] = "i",
            ['Č'] = "C",
            ['Ĭ'] = "I",
            ['Ō'] = "O",
            ['Ŭ'] = "U",
            ['ǌ'] = "nj",
            ['Ǭ'] = "O",
            ['Ȍ'] = "O",
            ['Í'] = "I",
            ['í'] = "i",
            ['č'] = "c",
            ['ĭ'] = "i",
            ['ō'] = "o",
            ['ŭ'] = "u",
            ['Ǎ'] = "A",
            ['ǭ'] = "o",
            ['ȍ'] = "o",
            ['Î'] = "I",
            ['î'] = "i",
            ['Ď'] = "D",
            ['Į'] = "I",
            ['Ŏ'] = "O",
            ['Ů'] = "U",
            ['ǎ'] = "a",
            ['Ǯ'] = "z",
            ['Ȏ'] = "O",
            ['Ï'] = "I",
            ['ï'] = "i",
            ['ď'] = "d",
            ['į'] = "i",
            ['ŏ'] = "o",
            ['ů'] = "u",
            ['Ǐ'] = "I",
            ['ǯ'] = "z",
            ['ȏ'] = "o",
            ['Ð'] = "D",
            ['ð'] = "d",
            ['Đ'] = "D",
            ['İ'] = "I",
            ['Ő'] = "O",
            ['Ű'] = "U",
            ['ǐ'] = "i",
            ['ǰ'] = "j",
            ['Ȑ'] = "R",
            ['Ñ'] = "N",
            ['ñ'] = "n",
            ['đ'] = "d",
            ['ı'] = "i",
            ['ő'] = "o",
            ['ű'] = "u",
            ['Ǒ'] = "O",
            ['Ǳ'] = "DZ",
            ['ȑ'] = "r",
            ['Ò'] = "O",
            ['ò'] = "o",
            ['Ē'] = "E",
            ['Ĳ'] = "LJ",
            ['Œ'] = "OE",
            ['Ų'] = "U",
            ['ǒ'] = "o",
            ['ǲ'] = "Dz",
            ['Ȓ'] = "R",
            ['Ó'] = "O",
            ['ó'] = "o",
            ['ē'] = "e",
            ['ĳ'] = "ij",
            ['œ'] = "oe",
            ['ų'] = "u",
            ['Ǔ'] = "U",
            ['ǳ'] = "dz",
            ['ȓ'] = "r",
            ['Ô'] = "O",
            ['ô'] = "o",
            ['Ĕ'] = "E",
            ['Ĵ'] = "J",
            ['Ŕ'] = "R",
            ['Ŵ'] = "W",
            ['ǔ'] = "u",
            ['Ǵ'] = "G",
            ['Ȕ'] = "U",
            ['Õ'] = "O",
            ['õ'] = "o",
            ['ĕ'] = "e",
            ['ĵ'] = "j",
            ['ŕ'] = "r",
            ['ŵ'] = "w",
            ['Ǖ'] = "U",
            ['ǵ'] = "g",
            ['ȕ'] = "u",
            ['Ö'] = "Oe",
            ['ö'] = "oe",
            ['Ė'] = "E",
            ['Ķ'] = "K",
            ['Ŗ'] = "R",
            ['Ŷ'] = "Y",
            ['ǖ'] = "u",
            ['Ƕ'] = "Hj",
            ['Ȗ'] = "U",
            ['ė'] = "e",
            ['ķ'] = "k",
            ['ŗ'] = "r",
            ['ŷ'] = "y",
            ['Ǘ'] = "U",
            ['ȗ'] = "u",
            ['Ø'] = "O",
            ['ø'] = "o",
            ['Ę'] = "E",
            ['ĸ'] = "k",
            ['Ř'] = "R",
            ['Ÿ'] = "Y",
            ['ǘ'] = "u",
            ['Ǹ'] = "N",
            ['Ș'] = "S",
            ['Ù'] = "U",
            ['ù'] = "u",
            ['ę'] = "e",
            ['Ĺ'] = "L",
            ['ř'] = "r",
            ['Ź'] = "Z",
            ['Ǚ'] = "U",
            ['ǹ'] = "n",
            ['ș'] = "s",
            ['Ú'] = "U",
            ['ú'] = "u",
            ['Ě'] = "E",
            ['ĺ'] = "l",
            ['Ś'] = "S",
            ['ź'] = "z",
            ['ǚ'] = "u",
            ['Ǻ'] = "A",
            ['Ț'] = "T",
            ['Û'] = "U",
            ['û'] = "u",
            ['ě'] = "e",
            ['Ļ'] = "L",
            ['ś'] = "s",
            ['Ż'] = "Z",
            ['Ǜ'] = "U",
            ['ǻ'] = "a",
            ['ț'] = "t",
            ['Ü'] = "Ue",
            ['ü'] = "ue",
            ['Ĝ'] = "G",
            ['ļ'] = "l",
            ['Ŝ'] = "S",
            ['ż'] = "z",
            ['ǜ'] = "u",
            ['Ǽ'] = "AE",
            ['Ȝ'] = "z",
            ['Ý'] = "Y",
            ['ý'] = "y",
            ['ĝ'] = "g",
            ['Ľ'] = "L",
            ['ŝ'] = "s",
            ['Ž'] = "Z",
            ['ǝ'] = "e",
            ['ǽ'] = "ae",
            ['ȝ'] = "z",
            ['Þ'] = "p",
            ['þ'] = "p",
            ['Ğ'] = "G",
            ['ľ'] = "L",
            ['Ş'] = "S",
            ['ž'] = "z",
            ['Ǟ'] = "A",
            ['Ǿ'] = "O",
            ['Ȟ'] = "H",
            ['ß'] = "ss",
            ['ÿ'] = "y",
            ['ğ'] = "g",
            ['Ŀ'] = "L",
            ['ş'] = "s",
            ['ſ'] = "l",
            ['ǟ'] = "a",
            ['ǿ'] = "o",
            ['ȟ'] = "h"
        };

        static StringExtensions()
        {
            LowerCaseDiacritics = Diacritics.ToDictionary(x => x.Key, x => x.Value.ToLowerInvariant());
        }

        public static bool IsSlug(this string value)
        {
            return value != null && SlugRegex.IsMatch(value);
        }

        public static bool IsPropertyName(this string value)
        {
            return value != null && PropertyNameRegex.IsMatch(value);
        }

        public static string WithFallback(this string value, string fallback)
        {
            return !string.IsNullOrWhiteSpace(value) ? value.Trim() : fallback;
        }

        public static string ToPascalCase(this string value)
        {
            return string.Concat(value.Split(new[] { '-', '_', ' ' }, StringSplitOptions.RemoveEmptyEntries).Select(c => char.ToUpper(c[0]) + c.Substring(1)));
        }

        public static string ToCamelCase(this string value)
        {
            value = value.ToPascalCase();

            return char.ToLower(value[0]) + value.Substring(1);
        }

        public static string Simplify(this string value, ISet<char> preserveHash = null, bool singleCharDiactric = false, char separator = '-')
        {
            var result = new StringBuilder(value.Length);

            var lastChar = (char)0;

            for (var i = 0; i < value.Length; i++)
            {
                var character = value[i];

                if (preserveHash?.Contains(character) == true)
                {
                    result.Append(character);
                }
                else if (char.IsLetter(character) || char.IsNumber(character))
                {
                    lastChar = character;

                    var lower = char.ToLowerInvariant(character);

                    if (LowerCaseDiacritics.TryGetValue(character, out string replacement))
                    {
                        if (singleCharDiactric)
                        {
                            result.Append(replacement[0]);
                        }
                        else
                        {
                            result.Append(replacement);
                        }
                    }
                    else
                    {
                        result.Append(lower);
                    }
                }
                else if ((i < value.Length - 1) && (i > 0 && lastChar != separator))
                {
                    lastChar = separator;

                    result.Append(separator);
                }
            }

            return result.ToString().Trim(separator);
        }

        public static string BuildFullUrl(this string baseUrl, string path, bool trailingSlash = false)
        {
            var url = $"{baseUrl.TrimEnd('/')}/{path.Trim('/')}";

            if (trailingSlash &&
                url.IndexOf("#", StringComparison.OrdinalIgnoreCase) < 0 &&
                url.IndexOf("?", StringComparison.OrdinalIgnoreCase) < 0 &&
                url.IndexOf(";", StringComparison.OrdinalIgnoreCase) < 0)
            {
                url = url + "/";
            }

            return url;
        }
    }
}
