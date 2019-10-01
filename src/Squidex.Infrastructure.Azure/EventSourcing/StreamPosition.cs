﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Squidex.Infrastructure.EventSourcing
{
    internal sealed class StreamPosition
    {
        public long Timestamp { get; }

        public long CommitOffset { get; }

        public long CommitSize { get; }

        public bool IsEndOfCommit
        {
            get { return CommitOffset == CommitSize - 1; }
        }

        public StreamPosition(long timestamp, long commitOffset, long commitSize)
        {
            Timestamp = timestamp;

            CommitOffset = commitOffset;
            CommitSize = commitSize;
        }

        public static implicit operator string(StreamPosition position)
        {
            var parts = new object[]
            {
                position.Timestamp,
                position.CommitOffset,
                position.CommitSize
            };

            return string.Join("-", parts);
        }

        public static implicit operator StreamPosition(string? position)
        {
            if (!string.IsNullOrWhiteSpace(position))
            {
                var parts = position.Split('-');

                return new StreamPosition(long.Parse(parts[0]), long.Parse(parts[1]), long.Parse(parts[2]));
            }

            return new StreamPosition(0, -1, -1);
        }
    }
}
