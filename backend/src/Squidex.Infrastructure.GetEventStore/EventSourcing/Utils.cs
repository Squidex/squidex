// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Globalization;
using System.Runtime.CompilerServices;
using EventStore.Client;

namespace Squidex.Infrastructure.EventSourcing;

public static class Utils
{
    public static StreamRevision ToRevision(this long version)
    {
        return StreamRevision.FromInt64(version);
    }

    public static StreamPosition ToPosition(this long version)
    {
        if (version <= 0)
        {
            return StreamPosition.Start;
        }

        return StreamPosition.FromInt64(version);
    }

    public static StreamPosition ToPosition(this string? position, bool inclusive)
    {
        if (string.IsNullOrWhiteSpace(position))
        {
            return StreamPosition.Start;
        }

        if (long.TryParse(position, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsedPosition))
        {
            if (!inclusive)
            {
                parsedPosition++;
            }

            return StreamPosition.FromInt64(parsedPosition);
        }

        return StreamPosition.Start;
    }

    public static async IAsyncEnumerable<StoredEvent> IgnoreNotFound(this IAsyncEnumerable<StoredEvent> source,
        [EnumeratorCancellation] CancellationToken ct = default)
    {
        var enumerator = source.GetAsyncEnumerator(ct);

        bool resultFound;
        try
        {
            resultFound = await enumerator.MoveNextAsync(ct);
        }
        catch (StreamNotFoundException)
        {
            resultFound = false;
        }

        if (!resultFound)
        {
            yield break;
        }

        yield return enumerator.Current;

        while (await enumerator.MoveNextAsync(ct))
        {
            ct.ThrowIfCancellationRequested();

            yield return enumerator.Current;
        }
    }
}
