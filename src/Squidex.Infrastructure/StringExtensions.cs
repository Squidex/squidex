// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
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
        private const char NullChar = (char)0;

        private static readonly Regex SlugRegex = new Regex("^[a-z0-9]+(\\-[a-z0-9]+)*$", RegexOptions.Compiled);
        private static readonly Regex EmailRegex = new Regex("^[a-zA-Z0-9.!#$%&’*+\\/=?^_`{|}~-]+@[a-zA-Z0-9-]+(?:.[a-zA-Z0-9-]+)*$", RegexOptions.Compiled);
        private static readonly Regex PropertyNameRegex = new Regex("^[a-zA-Z0-9]+(\\-[a-zA-Z0-9]+)*$", RegexOptions.Compiled);

        private static readonly Dictionary<char, string> LowerCaseDiacritics;
        private static readonly Dictionary<char, string> Diacritics = new Dictionary<char, string>
        {
            ['$'] = "dollar",
            ['%'] = "percent",
            ['&'] = "and",
            ['<'] = "less",
            ['>'] = "greater",
            ['|'] = "or",
            ['¢'] = "cent",
            ['£'] = "pound",
            ['¤'] = "currency",
            ['¥'] = "yen",
            ['©'] = "(c)",
            ['ª'] = "a",
            ['®'] = "(r)",
            ['º'] = "o",
            ['À'] = "A",
            ['Á'] = "A",
            ['Â'] = "A",
            ['Ã'] = "A",
            ['Ä'] = "AE",
            ['Å'] = "A",
            ['Æ'] = "AE",
            ['Ç'] = "C",
            ['È'] = "E",
            ['É'] = "E",
            ['Ê'] = "E",
            ['Ë'] = "E",
            ['Ì'] = "I",
            ['Í'] = "I",
            ['Î'] = "I",
            ['Ï'] = "I",
            ['Ð'] = "D",
            ['Ñ'] = "N",
            ['Ò'] = "O",
            ['Ó'] = "O",
            ['Ô'] = "O",
            ['Õ'] = "O",
            ['Ö'] = "OE",
            ['Ø'] = "O",
            ['Ù'] = "U",
            ['Ú'] = "U",
            ['Û'] = "U",
            ['Ü'] = "UE",
            ['Ý'] = "Y",
            ['Þ'] = "TH",
            ['ß'] = "ss",
            ['à'] = "a",
            ['á'] = "a",
            ['â'] = "a",
            ['ã'] = "a",
            ['ä'] = "ae",
            ['å'] = "a",
            ['æ'] = "ae",
            ['ç'] = "c",
            ['è'] = "e",
            ['é'] = "e",
            ['ê'] = "e",
            ['ë'] = "e",
            ['ì'] = "i",
            ['í'] = "i",
            ['î'] = "i",
            ['ï'] = "i",
            ['ð'] = "d",
            ['ñ'] = "n",
            ['ò'] = "o",
            ['ó'] = "o",
            ['ô'] = "o",
            ['õ'] = "o",
            ['ö'] = "oe",
            ['ø'] = "o",
            ['ù'] = "u",
            ['ú'] = "u",
            ['û'] = "u",
            ['ü'] = "ue",
            ['ý'] = "y",
            ['þ'] = "th",
            ['ÿ'] = "y",
            ['Ā'] = "A",
            ['ā'] = "a",
            ['Ă'] = "A",
            ['ă'] = "a",
            ['Ą'] = "A",
            ['ą'] = "a",
            ['Ć'] = "C",
            ['ć'] = "c",
            ['Č'] = "C",
            ['č'] = "c",
            ['Ď'] = "D",
            ['ď'] = "d",
            ['Đ'] = "DJ",
            ['đ'] = "dj",
            ['Ē'] = "E",
            ['ē'] = "e",
            ['Ė'] = "E",
            ['ė'] = "e",
            ['Ę'] = "e",
            ['ę'] = "e",
            ['Ě'] = "E",
            ['ě'] = "e",
            ['Ğ'] = "G",
            ['ğ'] = "g",
            ['Ģ'] = "G",
            ['ģ'] = "g",
            ['Ĩ'] = "I",
            ['ĩ'] = "i",
            ['Ī'] = "i",
            ['ī'] = "i",
            ['Į'] = "I",
            ['į'] = "i",
            ['İ'] = "I",
            ['ı'] = "i",
            ['Ķ'] = "k",
            ['ķ'] = "k",
            ['Ļ'] = "L",
            ['ļ'] = "l",
            ['Ľ'] = "L",
            ['ľ'] = "l",
            ['Ł'] = "L",
            ['ł'] = "l",
            ['Ń'] = "N",
            ['ń'] = "n",
            ['Ņ'] = "N",
            ['ņ'] = "n",
            ['Ň'] = "N",
            ['ň'] = "n",
            ['Ő'] = "O",
            ['ő'] = "o",
            ['Œ'] = "OE",
            ['œ'] = "oe",
            ['Ŕ'] = "R",
            ['ŕ'] = "r",
            ['Ř'] = "R",
            ['ř'] = "r",
            ['Ś'] = "S",
            ['ś'] = "s",
            ['Ş'] = "S",
            ['ş'] = "s",
            ['Š'] = "S",
            ['š'] = "s",
            ['Ţ'] = "T",
            ['ţ'] = "t",
            ['Ť'] = "T",
            ['ť'] = "t",
            ['Ũ'] = "U",
            ['ũ'] = "u",
            ['Ū'] = "u",
            ['ū'] = "u",
            ['Ů'] = "U",
            ['ů'] = "u",
            ['Ű'] = "U",
            ['ű'] = "u",
            ['Ų'] = "U",
            ['ų'] = "u",
            ['Ź'] = "Z",
            ['ź'] = "z",
            ['Ż'] = "Z",
            ['ż'] = "z",
            ['Ž'] = "Z",
            ['ž'] = "z",
            ['ƒ'] = "f",
            ['Ơ'] = "O",
            ['ơ'] = "o",
            ['Ư'] = "U",
            ['ư'] = "u",
            ['ǈ'] = "LJ",
            ['ǉ'] = "lj",
            ['ǋ'] = "NJ",
            ['ǌ'] = "nj",
            ['Ș'] = "S",
            ['ș'] = "s",
            ['Ț'] = "T",
            ['ț'] = "t",
            ['˚'] = "o",
            ['Ά'] = "A",
            ['Έ'] = "E",
            ['Ή'] = "H",
            ['Ί'] = "I",
            ['Ό'] = "O",
            ['Ύ'] = "Y",
            ['Ώ'] = "W",
            ['ΐ'] = "i",
            ['Α'] = "A",
            ['Β'] = "B",
            ['Γ'] = "G",
            ['Δ'] = "D",
            ['Ε'] = "E",
            ['Ζ'] = "Z",
            ['Η'] = "H",
            ['Θ'] = "8",
            ['Ι'] = "I",
            ['Κ'] = "K",
            ['Λ'] = "L",
            ['Μ'] = "M",
            ['Ν'] = "N",
            ['Ξ'] = "3",
            ['Ο'] = "O",
            ['Π'] = "P",
            ['Ρ'] = "R",
            ['Σ'] = "S",
            ['Τ'] = "T",
            ['Υ'] = "Y",
            ['Φ'] = "F",
            ['Χ'] = "X",
            ['Ψ'] = "PS",
            ['Ω'] = "W",
            ['Ϊ'] = "I",
            ['Ϋ'] = "Y",
            ['ά'] = "a",
            ['έ'] = "e",
            ['ή'] = "h",
            ['ί'] = "i",
            ['ΰ'] = "y",
            ['α'] = "a",
            ['β'] = "b",
            ['γ'] = "g",
            ['δ'] = "d",
            ['ε'] = "e",
            ['ζ'] = "z",
            ['η'] = "h",
            ['θ'] = "8",
            ['ι'] = "i",
            ['κ'] = "k",
            ['λ'] = "l",
            ['μ'] = "m",
            ['ν'] = "n",
            ['ξ'] = "3",
            ['ο'] = "o",
            ['π'] = "p",
            ['ρ'] = "r",
            ['ς'] = "s",
            ['σ'] = "s",
            ['τ'] = "t",
            ['υ'] = "y",
            ['φ'] = "f",
            ['χ'] = "x",
            ['ψ'] = "ps",
            ['ω'] = "w",
            ['ϊ'] = "i",
            ['ϋ'] = "y",
            ['ό'] = "o",
            ['ύ'] = "y",
            ['ώ'] = "w",
            ['Ё'] = "Yo",
            ['Ђ'] = "DJ",
            ['Є'] = "Ye",
            ['І'] = "I",
            ['Ї'] = "Yi",
            ['Ј'] = "J",
            ['Љ'] = "LJ",
            ['Њ'] = "NJ",
            ['Ћ'] = "C",
            ['Џ'] = "DZ",
            ['А'] = "A",
            ['Б'] = "B",
            ['В'] = "V",
            ['Г'] = "G",
            ['Д'] = "D",
            ['Е'] = "E",
            ['Ж'] = "Zh",
            ['З'] = "Z",
            ['И'] = "I",
            ['Й'] = "J",
            ['К'] = "K",
            ['Л'] = "L",
            ['М'] = "M",
            ['Н'] = "N",
            ['О'] = "O",
            ['П'] = "P",
            ['Р'] = "R",
            ['С'] = "S",
            ['Т'] = "T",
            ['У'] = "U",
            ['Ф'] = "F",
            ['Х'] = "H",
            ['Ц'] = "C",
            ['Ч'] = "Ch",
            ['Ш'] = "Sh",
            ['Щ'] = "Sh",
            ['Ъ'] = "U",
            ['Ы'] = "Y",
            ['Ь'] = "b",
            ['Э'] = "E",
            ['Ю'] = "Yu",
            ['Я'] = "Ya",
            ['а'] = "a",
            ['б'] = "b",
            ['в'] = "v",
            ['г'] = "g",
            ['д'] = "d",
            ['е'] = "e",
            ['ж'] = "zh",
            ['з'] = "z",
            ['и'] = "i",
            ['й'] = "j",
            ['к'] = "k",
            ['л'] = "l",
            ['м'] = "m",
            ['н'] = "n",
            ['о'] = "o",
            ['п'] = "p",
            ['р'] = "r",
            ['с'] = "s",
            ['т'] = "t",
            ['у'] = "u",
            ['ф'] = "f",
            ['х'] = "h",
            ['ц'] = "c",
            ['ч'] = "ch",
            ['ш'] = "sh",
            ['щ'] = "sh",
            ['ъ'] = "u",
            ['ы'] = "y",
            ['ь'] = "s",
            ['э'] = "e",
            ['ю'] = "yu",
            ['я'] = "ya",
            ['ё'] = "yo",
            ['ђ'] = "dj",
            ['є'] = "ye",
            ['і'] = "i",
            ['ї'] = "yi",
            ['ј'] = "j",
            ['љ'] = "lj",
            ['њ'] = "nj",
            ['ћ'] = "c",
            ['џ'] = "dz",
            ['Ґ'] = "G",
            ['ґ'] = "g",
            ['฿'] = "baht",
            ['ა'] = "a",
            ['ბ'] = "b",
            ['გ'] = "g",
            ['დ'] = "d",
            ['ე'] = "e",
            ['ვ'] = "v",
            ['ზ'] = "z",
            ['თ'] = "t",
            ['ი'] = "i",
            ['კ'] = "k",
            ['ლ'] = "l",
            ['მ'] = "m",
            ['ნ'] = "n",
            ['ო'] = "o",
            ['პ'] = "p",
            ['ჟ'] = "zh",
            ['რ'] = "r",
            ['ს'] = "s",
            ['ტ'] = "t",
            ['უ'] = "u",
            ['ფ'] = "f",
            ['ქ'] = "k",
            ['ღ'] = "gh",
            ['ყ'] = "q",
            ['შ'] = "sh",
            ['ჩ'] = "ch",
            ['ც'] = "ts",
            ['ძ'] = "dz",
            ['წ'] = "ts",
            ['ჭ'] = "ch",
            ['ხ'] = "kh",
            ['ჯ'] = "j",
            ['ჰ'] = "h",
            ['ẞ'] = "SS",
            ['Ạ'] = "A",
            ['ạ'] = "a",
            ['Ả'] = "A",
            ['ả'] = "a",
            ['Ấ'] = "A",
            ['ấ'] = "a",
            ['Ầ'] = "A",
            ['ầ'] = "a",
            ['Ẩ'] = "A",
            ['ẩ'] = "a",
            ['Ẫ'] = "A",
            ['ẫ'] = "a",
            ['Ậ'] = "A",
            ['ậ'] = "a",
            ['Ắ'] = "A",
            ['ắ'] = "a",
            ['Ằ'] = "A",
            ['ằ'] = "a",
            ['Ẳ'] = "A",
            ['ẳ'] = "a",
            ['Ẵ'] = "A",
            ['ẵ'] = "a",
            ['Ặ'] = "A",
            ['ặ'] = "a",
            ['Ẹ'] = "E",
            ['ẹ'] = "e",
            ['Ẻ'] = "E",
            ['ẻ'] = "e",
            ['Ẽ'] = "E",
            ['ẽ'] = "e",
            ['Ế'] = "E",
            ['ế'] = "e",
            ['Ề'] = "E",
            ['ề'] = "e",
            ['Ể'] = "E",
            ['ể'] = "e",
            ['Ễ'] = "E",
            ['ễ'] = "e",
            ['Ệ'] = "E",
            ['ệ'] = "e",
            ['Ỉ'] = "I",
            ['ỉ'] = "i",
            ['Ị'] = "I",
            ['ị'] = "i",
            ['Ọ'] = "O",
            ['ọ'] = "o",
            ['Ỏ'] = "O",
            ['ỏ'] = "o",
            ['Ố'] = "O",
            ['ố'] = "o",
            ['Ồ'] = "O",
            ['ồ'] = "o",
            ['Ổ'] = "O",
            ['ổ'] = "o",
            ['Ỗ'] = "O",
            ['ỗ'] = "o",
            ['Ộ'] = "O",
            ['ộ'] = "o",
            ['Ớ'] = "O",
            ['ớ'] = "o",
            ['Ờ'] = "O",
            ['ờ'] = "o",
            ['Ở'] = "O",
            ['ở'] = "o",
            ['Ỡ'] = "O",
            ['ỡ'] = "o",
            ['Ợ'] = "O",
            ['ợ'] = "o",
            ['Ụ'] = "U",
            ['ụ'] = "u",
            ['Ủ'] = "U",
            ['ủ'] = "u",
            ['Ứ'] = "U",
            ['ứ'] = "u",
            ['Ừ'] = "U",
            ['ừ'] = "u",
            ['Ử'] = "U",
            ['ử'] = "u",
            ['Ữ'] = "U",
            ['ữ'] = "u",
            ['Ự'] = "U",
            ['ự'] = "u",
            ['Ỳ'] = "Y",
            ['ỳ'] = "y",
            ['Ỵ'] = "Y",
            ['ỵ'] = "y",
            ['Ỷ'] = "Y",
            ['ỷ'] = "y",
            ['Ỹ'] = "Y",
            ['ỹ'] = "y",
            ['‘'] = "\'",
            ['’'] = "\'",
            ['“'] = "\\\"",
            ['”'] = "\\\"",
            ['†'] = "+",
            ['•'] = "*",
            ['…'] = "...",
            ['₠'] = "ecu",
            ['₢'] = "cruzeiro",
            ['₣'] = "french franc",
            ['₤'] = "lira",
            ['₥'] = "mill",
            ['₦'] = "naira",
            ['₧'] = "peseta",
            ['₨'] = "rupee",
            ['₩'] = "won",
            ['₪'] = "new shequel",
            ['₫'] = "dong",
            ['€'] = "euro",
            ['₭'] = "kip",
            ['₮'] = "tugrik",
            ['₯'] = "drachma",
            ['₰'] = "penny",
            ['₱'] = "peso",
            ['₲'] = "guarani",
            ['₳'] = "austral",
            ['₴'] = "hryvnia",
            ['₵'] = "cedi",
            ['₹'] = "indian rupee",
            ['₽'] = "russian ruble",
            ['₿'] = "bitcoin",
            ['℠'] = "sm",
            ['™'] = "tm",
            ['∂'] = "d",
            ['∆'] = "delta",
            ['∑'] = "sum",
            ['∞'] = "infinity",
            ['♥'] = "love",
            ['元'] = "yuan",
            ['円'] = "yen",
            ['﷼'] = "rial"
        };

        static StringExtensions()
        {
            LowerCaseDiacritics = Diacritics.ToDictionary(x => x.Key, x => x.Value.ToLowerInvariant());
        }

        public static bool IsSlug(this string? value)
        {
            return value != null && SlugRegex.IsMatch(value);
        }

        public static bool IsEmail(this string? value)
        {
            return value != null && EmailRegex.IsMatch(value);
        }

        public static bool IsPropertyName(this string? value)
        {
            return value != null && PropertyNameRegex.IsMatch(value);
        }

        public static string WithFallback(this string? value, string fallback)
        {
            return !string.IsNullOrWhiteSpace(value) ? value.Trim() : fallback;
        }

        public static string ToPascalCase(this string value)
        {
            if (value.Length == 0)
            {
                return string.Empty;
            }

            var sb = new StringBuilder(value.Length);

            var last = NullChar;
            var length = 0;

            for (var i = 0; i < value.Length; i++)
            {
                var c = value[i];

                if (c == '-' || c == '_' || c == ' ')
                {
                    if (last != NullChar)
                    {
                        sb.Append(char.ToUpperInvariant(last));
                    }

                    last = NullChar;
                    length = 0;
                }
                else
                {
                    if (length > 1)
                    {
                        sb.Append(c);
                    }
                    else if (length == 0)
                    {
                        last = c;
                    }
                    else
                    {
                        sb.Append(char.ToUpperInvariant(last));
                        sb.Append(c);

                        last = NullChar;
                    }

                    length++;
                }
            }

            if (last != NullChar)
            {
                sb.Append(char.ToUpperInvariant(last));
            }

            return sb.ToString();
        }

        public static string ToKebabCase(this string value)
        {
            if (value.Length == 0)
            {
                return string.Empty;
            }

            var sb = new StringBuilder(value.Length);

            var length = 0;

            for (var i = 0; i < value.Length; i++)
            {
                var c = value[i];

                if (c == '-' || c == '_' || c == ' ')
                {
                    length = 0;
                }
                else
                {
                    if (length > 0)
                    {
                        sb.Append(char.ToLowerInvariant(c));
                    }
                    else
                    {
                        if (sb.Length > 0)
                        {
                            sb.Append('-');
                        }

                        sb.Append(char.ToLowerInvariant(c));
                    }

                    length++;
                }
            }

            return sb.ToString();
        }

        public static string ToCamelCase(this string value)
        {
            if (value.Length == 0)
            {
                return string.Empty;
            }

            var sb = new StringBuilder(value.Length);

            var last = NullChar;
            var length = 0;

            for (var i = 0; i < value.Length; i++)
            {
                var c = value[i];

                if (c == '-' || c == '_' || c == ' ')
                {
                    if (last != NullChar)
                    {
                        if (sb.Length > 0)
                        {
                            sb.Append(char.ToUpperInvariant(last));
                        }
                        else
                        {
                            sb.Append(char.ToLowerInvariant(last));
                        }
                    }

                    last = NullChar;
                    length = 0;
                }
                else
                {
                    if (length > 1)
                    {
                        sb.Append(c);
                    }
                    else if (length == 0)
                    {
                        last = c;
                    }
                    else
                    {
                        if (sb.Length > 0)
                        {
                            sb.Append(char.ToUpperInvariant(last));
                        }
                        else
                        {
                            sb.Append(char.ToLowerInvariant(last));
                        }

                        sb.Append(c);

                        last = NullChar;
                    }

                    length++;
                }
            }

            if (last != NullChar)
            {
                if (sb.Length > 0)
                {
                    sb.Append(char.ToUpperInvariant(last));
                }
                else
                {
                    sb.Append(char.ToLowerInvariant(last));
                }
            }

            return sb.ToString();
        }

        public static string Slugify(this string value, ISet<char>? preserveHash = null, bool singleCharDiactric = false, char separator = '-')
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

                    if (LowerCaseDiacritics.TryGetValue(character, out var replacement))
                    {
                        if (singleCharDiactric && replacement.Length == 2)
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
            Guard.NotNull(path, nameof(path));

            var url = $"{baseUrl.TrimEnd('/')}/{path.Trim('/')}";

            if (trailingSlash &&
                url.IndexOf("#", StringComparison.OrdinalIgnoreCase) < 0 &&
                url.IndexOf("?", StringComparison.OrdinalIgnoreCase) < 0 &&
                url.IndexOf(";", StringComparison.OrdinalIgnoreCase) < 0)
            {
                url += "/";
            }

            return url;
        }

        public static string JoinNonEmpty(string separator, params string?[] parts)
        {
            Guard.NotNull(separator, nameof(separator));

            if (parts == null || parts.Length == 0)
            {
                return string.Empty;
            }

            return string.Join(separator, parts.Where(x => !string.IsNullOrWhiteSpace(x)));
        }
    }
}
