// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;

namespace Squidex.Infrastructure
{
    [TypeConverter(typeof(LanguageTypeConverter))]
    public partial record Language
    {
#pragma warning disable MA0023 // Add RegexOptions.ExplicitCapture
        private static readonly Regex CultureRegex = new Regex("^([a-z]{2})(\\-[a-z]{2})?$", RegexOptions.IgnoreCase);
#pragma warning restore MA0023 // Add RegexOptions.ExplicitCapture

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

        public static IReadOnlyCollection<Language> AllLanguages
        {
            get => AllLanguagesField.Values;
        }

        public string Iso2Code { get; }

        public string EnglishName
        {
            get => AllLanguagesNames.GetOrDefault(Iso2Code) ?? string.Empty;
        }

        private Language(string iso2Code)
        {
            Iso2Code = iso2Code;
        }

        public static bool IsValidLanguage(string iso2Code)
        {
            Guard.NotNull(iso2Code, nameof(iso2Code));

            return AllLanguagesField.ContainsKey(iso2Code);
        }

        public static bool TryGetLanguage(string iso2Code, [MaybeNullWhen(false)] out Language language)
        {
            Guard.NotNull(iso2Code, nameof(iso2Code));

            return AllLanguagesField.TryGetValue(iso2Code, out language!);
        }

        public static implicit operator string(Language language)
        {
            return language.Iso2Code;
        }

        public static implicit operator Language(string iso2Code)
        {
            return GetLanguage(iso2Code!);
        }

        public static Language? ParseOrNull(string input)
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

                input = match.Groups[1].Value;
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
