// ==========================================================================
//  Language.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

// ReSharper disable ConvertIfStatementToReturnStatement

namespace Squidex.Infrastructure
{
    public sealed partial class Language
    {
        private static readonly Regex CultureRegex = new Regex("^([a-z]{2})(\\-[a-z]{2})?$");
        private readonly string iso2Code;
        private readonly string englishName;
        private static readonly Dictionary<string, Language> AllLanguagesField = new Dictionary<string, Language>(StringComparer.OrdinalIgnoreCase);

        private static Language AddLanguage(string iso2Code, string englishName)
        {
            var language = new Language(iso2Code, englishName);

            AllLanguagesField[iso2Code] = language;

            return language;
        }

        public static Language GetLanguage(string iso2Code)
        {
            Guard.NotNullOrEmpty(iso2Code, nameof(iso2Code));

            try
            {
                return AllLanguagesField[iso2Code];
            }
            catch (KeyNotFoundException)
            {
                throw new NotSupportedException($"Language {iso2Code} is not supported");
            }
        }

        public static IEnumerable<Language> AllLanguages
        {
            get { return AllLanguagesField.Values; }
        }

        public string EnglishName
        {
            get { return englishName; }
        }

        public string Iso2Code
        {
            get { return iso2Code; }
        }

        private Language(string iso2Code, string englishName)
        {
            this.iso2Code = iso2Code;

            this.englishName = englishName;
        }

        public static bool IsValidLanguage(string iso2Code)
        {
            Guard.NotNullOrEmpty(iso2Code, nameof(iso2Code));

            return AllLanguagesField.ContainsKey(iso2Code);
        }

        public static bool TryGetLanguage(string iso2Code, out Language language)
        {
            Guard.NotNullOrEmpty(iso2Code, nameof(iso2Code));

            return AllLanguagesField.TryGetValue(iso2Code, out language);
        }

        public static implicit operator string(Language language)
        {
            return language?.Iso2Code;
        }

        public static implicit operator Language(string iso2Code)
        {
            return GetLanguage(iso2Code);
        }

        public static Language ParseOrNull(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                return null;
            }

            input = input.Trim();

            if (input.Length != 2)
            {
                var match = CultureRegex.Match(input);

                if (!match.Success)
                {
                    return null;
                }

                input = match.Groups[0].Value;
            }

            if (TryGetLanguage(input.ToLowerInvariant(), out var result))
            {
                return result;
            }

            return null;
        }

        public override string ToString()
        {
            return EnglishName;
        }
    }
}