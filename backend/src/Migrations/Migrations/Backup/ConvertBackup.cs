// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Entities.Jobs;
using Squidex.Infrastructure.Migrations;
using Squidex.Infrastructure.States;

namespace Migrations.Migrations.Backup;

public sealed class ConvertBackup : IMigration
{
    private readonly ISnapshotStore<BackupState> stateBackups;
    private readonly ISnapshotStore<JobsState> stateJobs;

    public ConvertBackup(
        ISnapshotStore<BackupState> stateBackups,
        ISnapshotStore<JobsState> stateJobs)
    {
        this.stateBackups = stateBackups;
        this.stateJobs = stateJobs;
    }

    public async Task UpdateAsync(
        CancellationToken ct)
    {
        await foreach (var state in stateBackups.ReadAllAsync(ct))
        {
            var job = state.Value.ToJob();

            await stateJobs.WriteAsync(new SnapshotWriteJob<JobsState>(state.Key, job, 0), ct);
        }

        await stateBackups.ClearAsync(ct);
    }
}
