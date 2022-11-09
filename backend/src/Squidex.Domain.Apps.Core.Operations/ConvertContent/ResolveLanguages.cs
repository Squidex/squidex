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

public sealed class ResolveLanguages : IContentFieldAfterConverter
{
    private readonly LanguagesConfig languages;
    private readonly bool resolveFallback;
    private readonly HashSet<string> languageCodes;

    public ResolveLanguages(LanguagesConfig languages, bool resolveFallback = true, params Language[] filteredLanguages)
    {
        this.languages = languages;

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

        this.resolveFallback = resolveFallback;
    }

    public ContentFieldData? ConvertFieldAfter(IRootField field, ContentFieldData source)
    {
        if (!field.Partitioning.Equals(Partitioning.Language))
        {
            return source;
        }

        if (source.TryGetNonNull(InvariantPartitioning.Key, out var value))
        {
            source = new ContentFieldData
            {
                [languages.Master] = value
            };
        }

        if (resolveFallback)
        {
            foreach (var languageCode in languageCodes)
            {
                if (source.TryGetNonNull(languageCode, out _))
                {
                    continue;
                }

                foreach (var fallback in languages.GetPriorities(languageCode))
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
                if (!languageCodes.Contains(key))
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
