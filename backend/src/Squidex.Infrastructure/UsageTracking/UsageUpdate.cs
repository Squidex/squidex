// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Squidex.Infrastructure.UsageTracking;

public struct UsageUpdate
{
    public DateTime Date;

    public string Key;

    public string Category;

    public Counters Counters;

    public UsageUpdate(DateTime date, string key, string category, Counters counters)
    {
        Key = key;
        Category = category;
        Counters = counters;
        Date = date;
    }
}
