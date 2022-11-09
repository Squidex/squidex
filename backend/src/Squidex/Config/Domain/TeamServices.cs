// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Entities.History;
using Squidex.Domain.Teams.Entities.Teams;

namespace Squidex.Config.Domain;

public static class TeamServices
{
    public static void AddSquidexTeams(this IServiceCollection services)
    {
        services.AddSingletonAs<TeamHistoryEventsCreator>()
            .As<IHistoryEventsCreator>();
    }
}
