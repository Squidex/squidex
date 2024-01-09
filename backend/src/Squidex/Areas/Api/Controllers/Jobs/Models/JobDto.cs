// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using NodaTime;
using Squidex.Domain.Apps.Entities.Jobs;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Collections;
using Squidex.Infrastructure.Reflection;
using Squidex.Web;

namespace Squidex.Areas.Api.Controllers.Jobs.Models;

public sealed class JobDto : Resource
{
    /// <summary>
    /// The ID of the job.
    /// </summary>
    public DomainId Id { get; set; }

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

    /// <summary>
    /// The name of the task.
    /// </summary>
    public string TaskName { get; set; }

    /// <summary>
    /// The description of the job.
    /// </summary>
    public string Description { get; set; }

    /// <summary>
    /// The arguments for the job.
    /// </summary>
    public ReadonlyDictionary<string, string> TaskArguments { get; set; }

    /// <summary>
    /// The list of log items.
    /// </summary>
    public List<JobLogMessageDto> Log { get; set; } = [];

    /// <summary>
    /// Indicates whether the job can be downloaded.
    /// </summary>
    public bool CanDownload { get; set; }

    public static JobDto FromDomain(Job job, Resources resources)
    {
        var result = SimpleMapper.Map(job, new JobDto());

        if (job.Log?.Count > 0)
        {
            result.Log = job.Log.Select(JobLogMessageDto.FromDomain).ToList();
        }

        result.TaskArguments = job.Arguments;

        return result.CreateLinks(job, resources);
    }

    private JobDto CreateLinks(Job job, Resources resources)
    {
        if (resources.CanDeleteJob)
        {
            var values = new { app = resources.App, id = Id };

            AddDeleteLink("delete",
                resources.Url<JobsController>(x => nameof(x.DeleteJob), values));
        }

        if (resources.CanDownloadJob && Status == JobStatus.Completed && job.File != null)
        {
            var values = new { appId = resources.AppId, id = Id };

            AddGetLink("download",
                resources.Url<JobsContentController>(x => nameof(x.GetJobContent), values));
        }

        return this;
    }
}
