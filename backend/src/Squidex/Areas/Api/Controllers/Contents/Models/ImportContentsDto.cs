﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Entities.Contents.Commands;
using Squidex.Infrastructure.Reflection;
using Squidex.Infrastructure.Validation;
using Squidex.Web;

namespace Squidex.Areas.Api.Controllers.Contents.Models;

[OpenApiRequest]
public sealed class ImportContentsDto
{
    /// <summary>
    /// The data to import.
    /// </summary>
    [LocalizedRequired]
    public List<ContentData> Datas { get; set; }

    /// <summary>
    /// True to automatically publish the content.
    /// </summary>
    [Obsolete("Use bulk endpoint now.")]
    public bool Publish { get; set; }

    /// <summary>
    /// True to turn off scripting for faster inserts. Default: true.
    /// </summary>
    public bool DoNotScript { get; set; } = true;

    /// <summary>
    /// True to turn off costly validation: Unique checks, asset checks and reference checks. Default: true.
    /// </summary>
    public bool OptimizeValidation { get; set; } = true;

    public BulkUpdateContents ToCommand()
    {
        var result = SimpleMapper.Map(this, new BulkUpdateContents());

        result.Jobs = Datas?.Select(x =>
        {
            var job = new BulkUpdateJob
            {
                Type = BulkUpdateContentType.Create,
                Data = x,
            };

#pragma warning disable CS0618 // Type or member is obsolete
            if (Publish)
            {
                job.Status = Status.Published;
            }
#pragma warning restore CS0618 // Type or member is obsolete

            return job;
        }).ToArray();

        return result;
    }
}
