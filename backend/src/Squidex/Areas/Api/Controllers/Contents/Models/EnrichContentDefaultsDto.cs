// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.AspNetCore.Mvc;
using Squidex.Domain.Apps.Entities.Contents.Commands;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Reflection;

namespace Squidex.Areas.Api.Controllers.Contents.Models;

public class EnrichContentDefaultsDto
{
    /// <summary>
    ///  True, to also enrich required fields. Default: false.
    /// </summary>
    [FromQuery(Name = "enrichRequiredFields")]
    public bool EnrichRequiredFields { get; set; }

    public EnrichContentDefaults ToCommand(DomainId id)
    {
        var command = SimpleMapper.Map(this, new EnrichContentDefaults { ContentId = id });

        return command;
    }
}
