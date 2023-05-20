// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Entities.Schemas.Commands;
using Squidex.Infrastructure.Reflection;
using Squidex.Web;

namespace Squidex.Areas.Api.Controllers.Schemas.Models;

[OpenApiRequest]
public sealed class ChangeCategoryDto
{
    /// <summary>
    /// The name of the category.
    /// </summary>
    public string? Name { get; set; }

    public ChangeCategory ToCommand()
    {
        return SimpleMapper.Map(this, new ChangeCategory());
    }
}
