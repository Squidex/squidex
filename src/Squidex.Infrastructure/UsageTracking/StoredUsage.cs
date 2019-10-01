// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;

namespace Squidex.Infrastructure.UsageTracking
{
    public sealed class StoredUsage
    {
        public string? Category { get; }

        public DateTime Date { get; }

        public Counters Counters { get;  }

        public StoredUsage(string? category, DateTime date, Counters counters)
        {
            Guard.NotNull(counters, nameof(counters));

            Category = category;
            Counters = counters;

            Date = date;
        }
    }
}
