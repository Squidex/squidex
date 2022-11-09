// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Squidex.Infrastructure.UsageTracking;

public sealed class Counters : Dictionary<string, double>
{
    public Counters()
    {
    }

    public Counters(Counters source)
        : base(source)
    {
    }

    public double Get(string name)
    {
        if (name == null)
        {
            return 0;
        }

        TryGetValue(name, out var value);

        return value;
    }

    public long GetInt64(string name)
    {
        if (name == null)
        {
            return 0;
        }

        TryGetValue(name, out var value);

        return (long)value;
    }

    public Counters SumUp(Counters counters)
    {
        foreach (var (key, value) in counters)
        {
            var newValue = value;

            if (TryGetValue(key, out var temp))
            {
                newValue += temp;
            }

            this[key] = newValue;
        }

        return this;
    }
}
