// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.AspNetCore.Mvc;
using Squidex.Domain.Apps.Entities.Schemas.Commands;
using Squidex.Infrastructure.Reflection;
using Squidex.Web;

namespace Squidex.Areas.Api.Controllers.Schemas.Models;

[OpenApiRequest]
public sealed class DeleteSchemaDto
{
    /// <summary>
    /// True to delete the schema and the contents permanently.
    /// </summary>
    [FromQuery(Name = "permanent")]
    public bool Permanent { get; set; }

    public DeleteSchema ToCommand()
    {
        return SimpleMapper.Map(this, new DeleteSchema());
    }
}
