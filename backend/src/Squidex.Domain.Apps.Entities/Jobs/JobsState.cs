// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure;
using Squidex.Infrastructure.Translations;

namespace Squidex.Domain.Apps.Entities.Jobs;

public sealed class JobsState
{
    public List<Job> Jobs { get; set; } = [];

    public void EnsureCanStart(IJobRunner runner)
    {
        if (Jobs.Exists(x => x.Status == JobStatus.Started))
        {
            throw new DomainException(T.Get("jobs.alreadyRunning"));
        }

        var max = runner.MaxJobs;

        var jobs = Jobs.Where(x => x.TaskName == runner.Name && x.File == null).Skip(max - 1).ToList();

        foreach (var job in jobs)
        {
            Jobs.Remove(job);
        }

        if (Jobs.Count(x => x.TaskName == runner.Name) >= max)
        {
            throw new DomainException(T.Get("jobs.maxReached", new { max }));
        }
    }
}
