// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.AspNetCore.Mvc;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Entities.Contents.Commands;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Reflection;
using StatusType = Squidex.Domain.Apps.Core.Contents.Status;

namespace Squidex.Areas.Api.Controllers.Contents.Models;

public class UpsertContentDto
{
    /// <summary>
    /// The full data for the content item.
    /// </summary>
    [FromBody]
    public ContentData Data { get; set; }

    /// <summary>
    /// The initial status.
    /// </summary>
    [FromQuery]
    public StatusType? Status { get; set; }

    /// <summary>
    /// Makes the update as patch.
    /// </summary>
    [FromQuery]
    public bool Patch { get; set; }

    /// <summary>
    /// True to automatically publish the content.
    /// </summary>
    [FromQuery]
    [Obsolete("Use 'status' query string now.")]
    public bool Publish { get; set; }

    public UpsertContent ToCommand(DomainId id)
    {
        var command = SimpleMapper.Map(this, new UpsertContent { ContentId = id });

#pragma warning disable CS0618 // Type or member is obsolete
        if (command.Status == null && Publish)
        {
            command.Status = StatusType.Published;
        }
#pragma warning restore CS0618 // Type or member is obsolete

        return command;
    }
}
