// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Entities.Teams.Commands;
using Squidex.Infrastructure.Reflection;
using Squidex.Infrastructure.Validation;

namespace Squidex.Areas.Api.Controllers.Teams.Models;

public sealed class UpdateTeamDto
{
    /// <summary>
    /// The name of the team.
    /// </summary>
    [LocalizedRequired]
    public string Name { get; set; }

    public UpdateTeam ToCommand()
    {
        return SimpleMapper.Map(this, new UpdateTeam());
    }
}
