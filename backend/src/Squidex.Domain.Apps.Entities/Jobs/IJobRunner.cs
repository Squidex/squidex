// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Squidex.Domain.Apps.Entities.Jobs;

public interface IJobRunner
{
    static string TaskName { get; }

    string Name { get; }

    int MaxJobs => 3;

    Task RunAsync(JobRunContext context,
        CancellationToken ct);

    Task DownloadAsync(Job job, Stream stream,
        CancellationToken ct)
    {
        return Task.CompletedTask;
    }

    Task CleanupAsync(Job job)
    {
        return Task.CompletedTask;
    }
}
