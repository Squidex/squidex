// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Diagnostics;

namespace Squidex.Infrastructure;

public readonly struct ValueStopwatch
{
    private const long TicksPerMillisecond = 10000;
    private const long TicksPerSecond = TicksPerMillisecond * 1000;

    private static readonly double TickFrequency;

    private readonly long startTime;

    static ValueStopwatch()
    {
        if (Stopwatch.IsHighResolution)
        {
            TickFrequency = (double)TicksPerSecond / Stopwatch.Frequency;
        }
    }

    public ValueStopwatch(long startTime)
    {
        this.startTime = startTime;
    }

    public static ValueStopwatch StartNew()
    {
        return new ValueStopwatch(Stopwatch.GetTimestamp());
    }

    public long Stop()
    {
        var elapsed = Stopwatch.GetTimestamp() - startTime;

        if (elapsed < 0)
        {
            return elapsed;
        }

        if (Stopwatch.IsHighResolution)
        {
            elapsed = unchecked((long)(elapsed * TickFrequency));
        }

        return elapsed / TicksPerMillisecond;
    }
}
