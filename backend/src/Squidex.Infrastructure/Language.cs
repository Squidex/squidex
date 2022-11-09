// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;

namespace Squidex.Infrastructure;

[TypeConverter(typeof(LanguageTypeConverter))]
public partial record Language
{
    private static readonly Regex CultureRegex = new Regex("^(?<Code>[a-z]{2})(\\-[a-z]{2})?$", RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture);

    public static Language GetLanguage(string iso2Code)
    {
        Guard.NotNullOrEmpty(iso2Code);

        if (LanguageByCode.TryGetValue(iso2Code, out var result))
        {
            return result;
        }

        return new Language(iso2Code.Trim());
    }

    public static IReadOnlyCollection<Language> AllLanguages
    {
        get => LanguageByCode.Values;
    }

    public string Iso2Code { get; }

    public string EnglishName
    {
        get => NamesEnglish.GetValueOrDefault(Iso2Code) ?? string.Empty;
    }

    public string NativeName
    {
        get => NamesNative.GetValueOrDefault(Iso2Code) ?? string.Empty;
    }

    private Language(string iso2Code)
    {
        Iso2Code = iso2Code;
    }

    public static bool IsDefault(string iso2Code)
    {
        Guard.NotNull(iso2Code);

        return LanguageByCode.ContainsKey(iso2Code);
    }

    public static bool TryGetLanguage(string iso2Code, [MaybeNullWhen(false)] out Language language)
    {
        Guard.NotNull(iso2Code);

        return LanguageByCode.TryGetValue(iso2Code, out language!);
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

            input = match.Groups["Code"].Value;
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
