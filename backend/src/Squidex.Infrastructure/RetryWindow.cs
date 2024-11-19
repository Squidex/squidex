// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using NodaTime;

namespace Squidex.Infrastructure;

public sealed class RetryWindow(TimeSpan windowDuration, int windowSize, IClock? clock = null)
{
    private readonly Duration windowDuration = Duration.FromTimeSpan(windowDuration);
    private readonly int windowSize = windowSize + 1;
    private readonly Queue<Instant> retries = new Queue<Instant>();
    private readonly IClock clock = clock ?? SystemClock.Instance;

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
