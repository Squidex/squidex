// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Entities.Jobs;

namespace Migrations.Migrations.Backup;

public sealed class BackupState
{
    public List<BackupJob> Jobs { get; set;  } = [];

    public JobsState ToJob()
    {
        var result = new JobsState
        {
            Jobs = Jobs.Select(ToState).ToList()
        };

        return result;
    }

    private static Job ToState(BackupJob source)
    {
        return new Job
        {
            Arguments = [],
            Id = source.Id,
            TaskName = "backup",
            Started = source.Started,
            Stopped = source.Stopped,
            File = new JobFile($"app-{source.Started:yyyy-MM-dd}.zip", "application/zip"),
            Status = source.Status switch
            {
                BackupStatus.Completed => JobStatus.Completed,
                BackupStatus.Created => JobStatus.Created,
                BackupStatus.Failed => JobStatus.Failed,
                BackupStatus.Started => JobStatus.Started,
                _ => JobStatus.Failed
            },
            Log =
            [
                new JobLogMessage(source.Stopped ?? source.Started, $"Total events: {source.HandledEvents}, assets: {source.HandledAssets}")
            ]
        };
    }
}
