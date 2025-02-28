﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Hosting;
using Squidex.Infrastructure.Timers;
using tusdotnet.Interfaces;

namespace Squidex.Domain.Apps.Entities.Assets;

public sealed class AssetCleanupProcess(ITusExpirationStore expirationStore) : IBackgroundProcess
{
    private CompletionTimer timer;

    public Task StartAsync(
        CancellationToken ct)
    {
        timer = new CompletionTimer((int)TimeSpan.FromMinutes(10).TotalMilliseconds, CleanupAsync);

        return Task.CompletedTask;
    }

    public Task StopAsync(
        CancellationToken ct)
    {
        return timer?.StopAsync() ?? Task.CompletedTask;
    }

    public Task CleanupAsync(
        CancellationToken ct)
    {
        return expirationStore.RemoveExpiredFilesAsync(ct);
    }
}
