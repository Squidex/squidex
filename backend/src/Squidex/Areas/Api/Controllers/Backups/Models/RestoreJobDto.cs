// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using NodaTime;
using Squidex.Domain.Apps.Entities.Backup;
using Squidex.Domain.Apps.Entities.Jobs;
using Squidex.Infrastructure.Reflection;

namespace Squidex.Areas.Api.Controllers.Backups.Models;

public sealed class RestoreJobDto
{
    /// <summary>
    /// The uri to load from.
    /// </summary>
    public Uri Url { get; set; }

    /// <summary>
    /// The status log.
    /// </summary>
    public List<string> Log { get; set; } = [];

    /// <summary>
    /// The time when the job has been started.
    /// </summary>
    public Instant Started { get; set; }

    /// <summary>
    /// The time when the job has been stopped.
    /// </summary>
    public Instant? Stopped { get; set; }

    /// <summary>
    /// The status of the operation.
    /// </summary>
    public JobStatus Status { get; set; }

    public static RestoreJobDto FromDomain(Job job)
    {
        var result = SimpleMapper.Map(job, new RestoreJobDto());

        if (job.Arguments.TryGetValue(RestoreJob.ArgUrl, out var urlString) && Uri.TryCreate(urlString, UriKind.Absolute, out var url))
        {
            result.Url = url;
        }

        result.Log = job.Log.Select(x => $"{x.Timestamp}: {x.Message}").ToList();

        return result;
    }
}
