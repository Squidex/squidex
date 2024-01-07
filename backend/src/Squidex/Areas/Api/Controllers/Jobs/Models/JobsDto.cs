// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Areas.Api.Controllers.Backups;
using Squidex.Domain.Apps.Entities.Jobs;
using Squidex.Web;

namespace Squidex.Areas.Api.Controllers.Jobs.Models;

public sealed class JobsDto : Resource
{
    /// <summary>
    /// The jobs.
    /// </summary>
    public JobDto[] Items { get; set; }

    public static JobsDto FromDomain(IEnumerable<Job> jobs, Resources resources)
    {
        var result = new JobsDto
        {
            Items = jobs.Select(x => JobDto.FromDomain(x, resources)).ToArray()
        };

        return result.CreateLinks(resources);
    }

    private JobsDto CreateLinks(Resources resources)
    {
        var values = new { app = resources.App };

        AddSelfLink(resources.Url<JobsController>(x => nameof(x.GetJobs), values));

        if (resources.CanCreateBackup)
        {
            AddPostLink("create/backups",
                resources.Url<BackupsController>(x => nameof(x.PostBackup), values));
        }

        return this;
    }
}
