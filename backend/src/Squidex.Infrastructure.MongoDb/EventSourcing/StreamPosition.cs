// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Text;
using Microsoft.Extensions.ObjectPool;
using MongoDB.Bson;

namespace Squidex.Infrastructure.EventSourcing
{
    internal sealed class StreamPosition
    {
        private static readonly ObjectPool<StringBuilder> StringBuilderPool =
            new DefaultObjectPool<StringBuilder>(new StringBuilderPooledObjectPolicy());

        public static readonly StreamPosition Empty = new StreamPosition(new BsonTimestamp(0, 0), -1, -1);

        public BsonTimestamp Timestamp { get; }

        public long CommitOffset { get; }

        public long CommitSize { get; }

        public bool IsEndOfCommit { get; }

        public StreamPosition(BsonTimestamp timestamp, long commitOffset, long commitSize)
        {
            Timestamp = timestamp;

            CommitOffset = commitOffset;
            CommitSize = commitSize;

            IsEndOfCommit = CommitOffset == CommitSize - 1;
        }

        public static implicit operator string(StreamPosition position)
        {
            var sb = StringBuilderPool.Get();
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
                StringBuilderPool.Return(sb);
            }
        }

        public static implicit operator StreamPosition(string? position)
        {
            if (!string.IsNullOrWhiteSpace(position))
            {
                var parts = position.Split('-');

                if (parts.Length == 4)
                {
                    return new StreamPosition(
                        new BsonTimestamp(int.Parse(parts[0]), int.Parse(parts[1])),
                        long.Parse(parts[2]),
                        long.Parse(parts[3]));
                }
            }

            return Empty;
        }
    }
}
