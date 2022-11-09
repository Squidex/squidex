// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using NodaTime;

namespace Squidex.Infrastructure;

public sealed class RetryWindow
{
    private readonly Duration windowDuration;
    private readonly int windowSize;
    private readonly Queue<Instant> retries = new Queue<Instant>();
    private readonly IClock clock;

    public RetryWindow(TimeSpan windowDuration, int windowSize, IClock? clock = null)
    {
        this.windowDuration = Duration.FromTimeSpan(windowDuration);
        this.windowSize = windowSize + 1;

        this.clock = clock ?? SystemClock.Instance;
    }

    public void Reset()
    {
        retries.Clear();
    }

    public bool CanRetryAfterFailure()
    {
        var now = clock.GetCurrentInstant();

        retries.Enqueue(now);

        while (retries.Count > windowSize)
        {
            retries.Dequeue();
        }

        return retries.Count < windowSize || (retries.Count > 0 && (now - retries.Peek()) > windowDuration);
    }
}
