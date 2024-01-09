// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using NodaTime;
using Squidex.Areas.Api.Controllers.Jobs;
using Squidex.Domain.Apps.Entities.Jobs;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Reflection;
using Squidex.Web;

namespace Squidex.Areas.Api.Controllers.Backups.Models;

[Obsolete("Use Jobs endpoint.")]
public sealed class BackupJobDto : Resource
{
    /// <summary>
    /// The ID of the backup job.
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
    /// The number of handled events.
    /// </summary>
    public int HandledEvents { get; set; }

    /// <summary>
    /// The number of handled assets.
    /// </summary>
    public int HandledAssets { get; set; }

    /// <summary>
    /// The status of the operation.
    /// </summary>
    public JobStatus Status { get; set; }

    public static BackupJobDto FromDomain(Job job, Resources resources)
    {
        var result = SimpleMapper.Map(job, new BackupJobDto());

        return result.CreateLinks(job, resources);
    }

    private BackupJobDto CreateLinks(Job job, Resources resources)
    {
        if (resources.CanDeleteJob)
        {
            var values = new { app = resources.App, id = Id };

            AddDeleteLink("delete",
                resources.Url<BackupsController>(x => nameof(x.DeleteBackup), values));
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
