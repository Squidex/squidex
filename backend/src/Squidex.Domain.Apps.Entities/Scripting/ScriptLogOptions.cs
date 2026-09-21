// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Squidex.Domain.Apps.Entities.Scripting;

public sealed class ScriptLogOptions
{
    public bool Enabled { get; set; } = true;

    public int MaxItemsPerApp { get; set; } = 100;

    public int RetentionInDays { get; set; } = 7;

    public int BatchSize { get; set; } = 500;

    public int WriteIntervall { get; set; } = 1000;

    public TimeSpan CleanupIntervall { get; set; } = TimeSpan.FromMinutes(10);

    public int MaxPendingItems { get; set; } = 10_000;
}
