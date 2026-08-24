// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Squidex.Infrastructure.Log;

public sealed class RequestLogStoreOptions
{
    public bool StoreEnabled { get; set; } = true;

    public int StoreRetentionInDays { get; set; } = 90;

    public int BatchSize { get; set; } = 1000;

    public int WriteIntervall { get; set; } = 1000;

    // Requests are only written every few seconds. When the repository is not available the pending
    // entries would grow without a limit, therefore they are dropped when this size is reached.
    public int MaxPendingItems { get; set; } = 50_000;
}
