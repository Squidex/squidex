// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Entities.Apps.Commands;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Reflection;

namespace Squidex.Areas.Api.Controllers.Apps.Models;

public sealed class TransferToTeamDto
{
    /// <summary>
    /// The ID of the team.
    /// </summary>
    public DomainId? TeamId { get; set; }

    public TransferToTeam ToCommand()
    {
        return SimpleMapper.Map(this, new TransferToTeam());
    }
}
