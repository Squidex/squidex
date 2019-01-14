// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using NodaTime;

namespace Squidex.Infrastructure.Log
{
    public sealed class TimestampLogAppender : ILogAppender
    {
        private readonly IClock clock;

        public TimestampLogAppender(IClock clock = null)
        {
            this.clock = clock ?? SystemClock.Instance;
        }

        public void Append(IObjectWriter writer)
        {
            writer.WriteProperty("timestamp", clock.GetCurrentInstant());
        }
    }
}
