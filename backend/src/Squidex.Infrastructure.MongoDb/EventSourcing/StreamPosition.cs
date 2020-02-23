// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Text;
using MongoDB.Bson;

namespace Squidex.Infrastructure.EventSourcing
{
    internal sealed class StreamPosition
    {
        private static readonly BsonTimestamp EmptyTimestamp = new BsonTimestamp(946681200, 0);

        public BsonTimestamp Timestamp { get; }

        public long CommitOffset { get; }

        public long CommitSize { get; }

        public bool IsEndOfCommit
        {
            get { return CommitOffset == CommitSize - 1; }
        }

        public StreamPosition(BsonTimestamp timestamp, long commitOffset, long commitSize)
        {
            Timestamp = timestamp;

            CommitOffset = commitOffset;
            CommitSize = commitSize;
        }

        public static implicit operator string(StreamPosition position)
        {
            var sb = new StringBuilder();

            sb.Append(position.Timestamp.Timestamp);
            sb.Append(",");
            sb.Append(position.Timestamp.Increment);
            sb.Append(",");
            sb.Append(position.CommitOffset);
            sb.Append(",");
            sb.Append(position.CommitSize);

            return sb.ToString();
        }

        public static implicit operator StreamPosition(string? position)
        {
            if (!string.IsNullOrWhiteSpace(position))
            {
                var parts = position.Split('-');

                return new StreamPosition(new BsonTimestamp(int.Parse(parts[0]), int.Parse(parts[1])), long.Parse(parts[2]), long.Parse(parts[3]));
            }

            return new StreamPosition(EmptyTimestamp, -1, -1);
        }
    }
}
