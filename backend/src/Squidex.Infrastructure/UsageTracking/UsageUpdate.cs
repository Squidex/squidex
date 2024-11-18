// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Squidex.Infrastructure.UsageTracking;

public struct UsageUpdate(DateOnly date, string key, string category, Counters counters)
{
    public DateOnly Date = date;

    public string Key = key;

    public string Category = category;

    public Counters Counters = counters;
}
