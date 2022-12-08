// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Diagnostics.CodeAnalysis;
using Squidex.Domain.Apps.Core.Apps;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities.Apps;

public static class AppExtensions
{
    public static NamedId<DomainId> NamedId(this IAppEntity app)
    {
        return new NamedId<DomainId>(app.Id, app.Name);
    }

    public static string DisplayName(this IAppEntity app)
    {
        return app.Label.Or(app.Name);
    }

    public static bool TryGetContributorRole(this IAppEntity app, string id, bool isFrontend, [MaybeNullWhen(false)] out Role role)
    {
        role = null;

        return app.Contributors.TryGetValue(id, out var roleName) && app.TryGetRole(roleName, isFrontend, out role);
    }

    public static bool TryGetClientRole(this IAppEntity app, string id, bool isFrontend, [MaybeNullWhen(false)] out Role role)
    {
        role = null;

        return app.Clients.TryGetValue(id, out var client) && app.TryGetRole(client.Role, isFrontend, out role);
    }

    public static bool TryGetRole(this IAppEntity app, string roleName, bool isFrontend, [MaybeNullWhen(false)] out Role role)
    {
        return app.Roles.TryGet(app.Name, roleName, isFrontend, out role);
    }
}
