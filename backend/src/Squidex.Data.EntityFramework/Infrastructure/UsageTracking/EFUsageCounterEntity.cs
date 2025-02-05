// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Squidex.Infrastructure.UsageTracking;

[Table("Counter")]
[PrimaryKey(nameof(Key), nameof(Date), nameof(Category), nameof(CounterKey))]
public sealed class EFUsageCounterEntity
{
    public DateTime Date { get; set; }

    public string Key { get; set; }

    public string Category { get; set; }

    public string CounterKey { get; set; }

    public double CounterValue { get; set; }
}
