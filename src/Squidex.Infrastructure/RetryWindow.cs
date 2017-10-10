// ==========================================================================
//  RetryWindow.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Collections.Generic;

namespace Squidex.Infrastructure
{
    public sealed class RetryWindow
    {
        private readonly TimeSpan windowDuration;
        private readonly int windowSize;
        private readonly Queue<DateTime> retries = new Queue<DateTime>();

        public RetryWindow(TimeSpan windowDuration, int windowSize)
        {
            this.windowDuration = windowDuration;
            this.windowSize = windowSize + 1;
        }

        public void Reset()
        {
            retries.Clear();
        }

        public bool CanRetryAfterFailure()
        {
            return CanRetryAfterFailure(DateTime.UtcNow);
        }

        public bool CanRetryAfterFailure(DateTime utcNow)
        {
            retries.Enqueue(utcNow);

            while (retries.Count > windowSize)
            {
                retries.Dequeue();
            }

            return retries.Count < windowSize || (retries.Count > 0 && (utcNow - retries.Peek()) > windowDuration);
        }
    }
}
