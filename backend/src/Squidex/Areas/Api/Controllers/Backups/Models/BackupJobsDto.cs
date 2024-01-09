// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Entities.Jobs;
using Squidex.Web;

namespace Squidex.Areas.Api.Controllers.Backups.Models;

[Obsolete("Use Jobs endpoint.")]
public sealed class BackupJobsDto : Resource
{
    /// <summary>
    /// The backups.
    /// </summary>
    public BackupJobDto[] Items { get; set; }

    public static BackupJobsDto FromDomain(IEnumerable<Job> jobs, Resources resources)
    {
        var result = new BackupJobsDto
        {
            Items = jobs.Select(x => BackupJobDto.FromDomain(x, resources)).ToArray()
        };

        return result.CreateLinks(resources);
    }

    private BackupJobsDto CreateLinks(Resources resources)
    {
        var values = new { app = resources.App };

        AddSelfLink(resources.Url<BackupsController>(x => nameof(x.GetBackups), values));

        if (resources.CanCreateBackup)
        {
            AddPostLink("create",
                resources.Url<BackupsController>(x => nameof(x.PostBackup), values));
        }

        return this;
    }
}
