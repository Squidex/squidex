// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Apps;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Core.Schemas;

namespace Squidex.Domain.Apps.Core.ConvertContent;

public sealed class ResolveFromPreviousPartitioning : IContentFieldConverter
{
    private readonly LanguagesConfig languages;

    public ResolveFromPreviousPartitioning(LanguagesConfig languages)
    {
        this.languages = languages;
    }

    public ContentFieldData? ConvertFieldAfter(IRootField field, ContentFieldData source)
    {
        if (field.Partitioning.Equals(Partitioning.Invariant))
        {
            return ResolveInvariant(source);
        }
        else
        {
            return ResolveLocalized(source);
        }
    }

    private ContentFieldData ResolveLocalized(ContentFieldData source)
    {
        if (source.TryGetNonNull(languages.Master, out _))
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

        return source;
    }

    private ContentFieldData ResolveInvariant(ContentFieldData source)
    {
        if (source.TryGetNonNull(InvariantPartitioning.Key, out _))
        {
            return source;
        }

        if (source.TryGetNonNull(languages.Master, out var value))
        {
            source.Clear();
            source[InvariantPartitioning.Key] = value;

            return source;
        }

        if (source.Count > 0)
        {
            var first = source.First().Value;

            source.Clear();
            source[InvariantPartitioning.Key] = first;

            return source;
        }

        source.Clear();

        return source;
    }
}
