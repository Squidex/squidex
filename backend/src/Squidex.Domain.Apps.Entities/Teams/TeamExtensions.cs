// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Diagnostics.CodeAnalysis;

namespace Squidex.Domain.Apps.Entities.Teams;

public static class TeamExtensions
{
    public static bool TryGetContributorRole(this ITeamEntity app, string id, [MaybeNullWhen(false)] out string role)
    {
        return app.Contributors.TryGetValue(id, out role);
    }
}
