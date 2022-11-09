// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Entities.Apps.Commands;
using Squidex.Infrastructure.Reflection;
using Squidex.Infrastructure.Validation;

namespace Squidex.Areas.Api.Controllers.Apps.Models;

public sealed class CreateAppDto
{
    /// <summary>
    /// The name of the app.
    /// </summary>
    [LocalizedRequired]
    [LocalizedRegularExpression("^[a-z0-9]+(\\-[a-z0-9]+)*$")]
    public string Name { get; set; }

    /// <summary>
    /// Initialize the app with the inbuilt template.
    /// </summary>
    public string? Template { get; set; }

    public CreateApp ToCommand()
    {
        return SimpleMapper.Map(this, new CreateApp());
    }
}
