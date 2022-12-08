// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using NodaTime;
using Squidex.Domain.Apps.Entities.Backup;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Reflection;
using Squidex.Web;

namespace Squidex.Areas.Api.Controllers.Backups.Models;

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

    public static BackupJobDto FromDomain(IBackupJob backup, Resources resources)
    {
        var result = SimpleMapper.Map(backup, new BackupJobDto());

        return result.CreateLinks(resources);
    }

    private BackupJobDto CreateLinks(Resources resources)
    {
        if (resources.CanDeleteBackup)
        {
            var values = new { app = resources.App, id = Id };

            AddDeleteLink("delete",
                resources.Url<BackupsController>(x => nameof(x.DeleteBackup), values));
        }

        if (resources.CanDownloadBackup)
        {
            var values = new { app = resources.App, appId = resources.AppId, id = Id };

            AddGetLink("download",
                resources.Url<BackupContentController>(x => nameof(x.GetBackupContentV2), values));
        }

        return this;
    }
}
