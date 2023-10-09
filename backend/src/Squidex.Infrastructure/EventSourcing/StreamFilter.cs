// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure.Collections;

namespace Squidex.Infrastructure.EventSourcing;

public readonly record struct StreamFilter
{
    public ReadonlyList<string>? Prefixes { get; }

    public StreamFilterKind Kind { get; }

    public StreamFilter(StreamFilterKind kind, params string[] prefixes)
    {
        Kind = kind;

        if (prefixes.Length > 0)
        {
            Prefixes = prefixes.ToReadonlyList();
        }
    }

    public static StreamFilter Prefix(params string[] prefixes)
    {
        return new StreamFilter(StreamFilterKind.MatchStart, prefixes);
    }

    public static StreamFilter Name(params string[] prefixes)
    {
        return new StreamFilter(StreamFilterKind.MatchFull, prefixes);
    }

    public static StreamFilter All()
    {
        return default;
    }
}
