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
        private readonly Func<DateTime> timestamp;

        public TimestampLogAppender()
            : this(() => DateTime.UtcNow)
        {
        }

        public TimestampLogAppender(Func<DateTime> timestamp)
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
