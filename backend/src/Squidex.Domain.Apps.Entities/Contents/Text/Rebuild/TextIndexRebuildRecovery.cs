// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.Extensions.Logging;
using Squidex.Domain.Apps.Entities.Jobs;
using Squidex.Hosting;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Timers;

namespace Squidex.Domain.Apps.Entities.Contents.Text.Rebuild;

public sealed class TextIndexRebuildRecovery(
    TextIndexRebuildMarkers markers,
    TextIndexRebuildCoordinator coordinator,
    IAppProvider appProvider,
    IJobService jobService,
    ILogger<TextIndexRebuildRecovery> log)
    : IBackgroundProcess
{
    private static readonly RefToken Actor = RefToken.Client("TextIndexer");
    private CompletionTimer? timer;

    public Task StartAsync(
        CancellationToken ct)
    {
        // Also retry failed attempts, e.g. when another job is running for the app.
        timer = new CompletionTimer((int)TimeSpan.FromMinutes(1).TotalMilliseconds, RecoverAsync);

        return Task.CompletedTask;
    }

    public Task StopAsync(
        CancellationToken ct)
    {
        return timer?.StopAsync() ?? Task.CompletedTask;
    }

    public async Task RecoverAsync(
        CancellationToken ct)
    {
        foreach (var marker in await markers.GetAsync(ct))
        {
            // The rebuild is still running, therefore there is nothing to recover.
            if (await coordinator.IsRebuildingAsync(marker.AppId, ct))
            {
                continue;
            }

            try
            {
                await RestartAsync(marker, ct);
            }
            catch (Exception ex)
            {
                log.LogWarning(ex, "Failed to restart rebuild of full text index for app {appId}.", marker.AppId);
            }
        }
    }

    private async Task RestartAsync(TextIndexRebuildMarker marker,
        CancellationToken ct)
    {
        var app = await appProvider.GetAppAsync(marker.AppId, true, ct);
        if (app == null)
        {
            await markers.RemoveAsync(marker.AppId, ct);
            return;
        }

        var schema = marker.SchemaId != null ? await appProvider.GetSchemaAsync(app.Id, marker.SchemaId.Value, true, ct) : null;

        // A full rebuild also covers the contents that have been skipped before the rebuild was interrupted.
        await jobService.StartAsync(app.Id, RebuildTextIndexJob.BuildRequest(Actor, app, schema), ct);
    }
}
