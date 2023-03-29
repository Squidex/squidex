// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Globalization;
using MongoDB.Bson;
using NodaTime;
using Squidex.Infrastructure.ObjectPool;

#pragma warning disable SA1313 // Parameter names should begin with lower-case letter
#pragma warning disable RECS0082 // Parameter has the same name as a member and hides it

namespace Squidex.Infrastructure.EventSourcing;

internal sealed record StreamPosition(BsonTimestamp Timestamp, long CommitOffset, long CommitSize)
{
    public static readonly StreamPosition Empty = new StreamPosition(new BsonTimestamp(0, 0), -1, -1);

    public bool IsEndOfCommit => CommitOffset == CommitSize - 1;

    public static implicit operator string(StreamPosition position)
    {
        var sb = DefaultPools.StringBuilder.Get();
        try
        {
            sb.Append(position.Timestamp.Timestamp);
            sb.Append('-');
            sb.Append(position.Timestamp.Increment);
            sb.Append('-');
            sb.Append(position.CommitOffset);
            sb.Append('-');
            sb.Append(position.CommitSize);

            return sb.ToString();
        }
        finally
        {
            DefaultPools.StringBuilder.Return(sb);
        }
    }

    public static implicit operator StreamPosition(string? position)
    {
        if (!string.IsNullOrWhiteSpace(position))
        {
            var parts = position.Split('-');

            if (parts.Length == 4)
            {
                var culture = CultureInfo.InvariantCulture;

                return new StreamPosition(
                    new BsonTimestamp(
                        int.Parse(parts[0], NumberStyles.Integer, culture),
                        int.Parse(parts[1], NumberStyles.Integer, culture)),
                    long.Parse(parts[2], NumberStyles.Integer, culture),
                    long.Parse(parts[3], NumberStyles.Integer, culture));
            }
        }

        return Empty;
    }

    public static implicit operator StreamPosition(Instant timestamp)
    {
        if (timestamp != default)
        {
            return new StreamPosition(
                new BsonTimestamp((int)timestamp.ToUnixTimeSeconds(), 0),
                0,
                0);
        }

        return Empty;
    }
}
