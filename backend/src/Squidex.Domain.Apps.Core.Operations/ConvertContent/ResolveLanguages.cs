// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Apps;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Core.ConvertContent;

public sealed class ResolveLanguages : IContentFieldConverter
{
    private readonly Dictionary<string, string[]> languagesWithFallbacks;

    public bool ResolveFallback { get; init; }

    public HashSet<string>? FieldNames { get; init; }

    public ResolveLanguages(LanguagesConfig languages, params Language[] filteredLanguages)
    {
        HashSet<string> languageCodes;

        if (filteredLanguages?.Length > 0)
        {
            languageCodes = languages.AllKeys.Intersect(filteredLanguages.Select(x => x.Iso2Code)).ToHashSet();
        }
        else
        {
            languageCodes = languages.AllKeys.ToHashSet();
        }

        if (languageCodes.Count == 0)
        {
            languageCodes.Add(languages.Master);
        }

        languagesWithFallbacks =
            languageCodes.ToDictionary(
                language => language,
                language => languages.GetPriorities(language).Where(l => l != language).ToArray());
    }

    public ContentFieldData? ConvertFieldAfter(IRootField field, ContentFieldData source)
    {
        if (FieldNames?.Contains(field.Name) == false)
        {
            // If the fields are set, we only enrich the given matching field names.
            return source;
        }

        if (!field.Partitioning.Equals(Partitioning.Language))
        {
            return source;
        }

        if (ResolveFallback)
        {
            foreach (var (languageCode, fallbacks) in languagesWithFallbacks)
            {
                if (source.TryGetNonNull(languageCode, out _))
                {
                    continue;
                }

                foreach (var fallback in fallbacks)
                {
                    if (source.TryGetNonNull(fallback, out var fallbackValue))
                    {
                        source[languageCode] = fallbackValue;
                        break;
                    }
                }
            }
        }

        while (true)
        {
            var isRemoved = false;

            foreach (var (key, _) in source)
            {
                if (!languagesWithFallbacks.ContainsKey(key))
                {
                    source.Remove(key);
                    isRemoved = true;
                    break;
                }
            }

            if (!isRemoved)
            {
                break;
            }
        }

        return source;
    }
}
