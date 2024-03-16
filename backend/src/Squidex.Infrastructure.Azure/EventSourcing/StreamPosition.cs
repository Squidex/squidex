// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure.ObjectPool;

namespace Squidex.Infrastructure.EventSourcing
{
    internal sealed class StreamPosition
    {
        public static readonly StreamPosition Empty = new StreamPosition(0, -1, -1);

        public long Timestamp { get; }

        public long CommitOffset { get; }

        public long CommitSize { get; }

        public bool IsEndOfCommit
        {
            get => CommitOffset == CommitSize - 1;
        }

        public StreamPosition(long timestamp, long commitOffset, long commitSize)
        {
            Timestamp = timestamp;

            CommitOffset = commitOffset;
            CommitSize = commitSize;
        }

        public static implicit operator string(StreamPosition position)
        {
            var sb = DefaultPools.StringBuilder.Get();
            try
            {
                sb.Append(position.Timestamp);
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

                if (parts.Length == 3)
                {
                    return new StreamPosition(
                    long.Parse(parts[0]),
                    long.Parse(parts[1]),
                    long.Parse(parts[2]));
                }
            }

            return Empty;
        }
    }
}
