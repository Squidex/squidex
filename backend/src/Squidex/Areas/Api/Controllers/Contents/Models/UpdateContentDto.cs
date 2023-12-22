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
using Squidex.Web;

namespace Squidex.Areas.Api.Controllers.Contents.Models;

[OpenApiRequest]
public class UpdateContentDto
{
    /// <summary>
    /// The full data for the content item.
    /// </summary>
    [FromBody]
    public ContentData Data { get; set; }

    /// <summary>
    /// Enrich the content with defaults.
    /// </summary>
    [FromQuery(Name = "enrichDefaults")]
    public bool EnrichDefaults { get; set; }

    public UpdateContent ToCommand(DomainId id)
    {
        return SimpleMapper.Map(this, new UpdateContent { ContentId = id });
    }
}
