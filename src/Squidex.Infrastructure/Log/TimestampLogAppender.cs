// ==========================================================================
//  TimestampLogAppender.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;

namespace Squidex.Infrastructure.Log
{
    public sealed class TimestampLogAppender : ILogAppender
    {
        private readonly Func<long> timestamp;

        public TimestampLogAppender()
            : this(() => DateTimeOffset.UtcNow.ToUnixTimeSeconds())
        {
        }

        public TimestampLogAppender(Func<long> timestamp)
        {
            Guard.NotNull(timestamp, nameof(timestamp));

            this.timestamp = timestamp;
        }

        public void Append(IObjectWriter writer)
        {
            writer.WriteProperty("timestamp", timestamp());
        }
    }
}
