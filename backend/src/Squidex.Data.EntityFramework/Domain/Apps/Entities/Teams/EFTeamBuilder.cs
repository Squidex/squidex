// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Teams;
using Squidex.Domain.Apps.Entities.Teams;
using Squidex.Infrastructure.Json;
using Squidex.Infrastructure.States;

namespace Microsoft.EntityFrameworkCore;

public static class EFTeamBuilder
{
    public static void UseTeams(this ModelBuilder builder, IJsonSerializer jsonSerializer, string? jsonColumn)
    {
        builder.UseSnapshot<Team, EFTeamEntity>(jsonSerializer, jsonColumn);
    }
}
