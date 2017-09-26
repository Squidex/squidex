// ==========================================================================
//  StreamPosition.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using MongoDB.Bson;

namespace Squidex.Infrastructure.CQRS.Events
{
    public sealed class StreamPosition
    {
        private static readonly BsonTimestamp EmptyTimestamp = new BsonTimestamp(0);

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
            var parts = new object[]
            {
                position.Timestamp.Timestamp,
                position.Timestamp.Increment,
                position.CommitOffset,
                position.CommitSize
            };

            return string.Join("-", parts);
        }

        public static implicit operator StreamPosition(string position)
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
