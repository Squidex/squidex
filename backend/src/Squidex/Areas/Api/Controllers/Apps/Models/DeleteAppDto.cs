// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.AspNetCore.Mvc;
using Squidex.Domain.Apps.Entities.Apps.Commands;
using Squidex.Infrastructure.Reflection;
using Squidex.Web;

namespace Squidex.Areas.Api.Controllers.Apps.Models;

[OpenApiRequest]
public sealed class DeleteAppDto
{
    /// <summary>
    /// True to delete the app permanently.
    /// </summary>
    [FromQuery(Name = "permanent")]
    public bool Permanent { get; set; }

    public DeleteApp ToCommand()
    {
        return SimpleMapper.Map(this, new DeleteApp());
    }
}
