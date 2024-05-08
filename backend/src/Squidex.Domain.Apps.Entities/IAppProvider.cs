// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Apps;
using Squidex.Domain.Apps.Core.Rules;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Domain.Apps.Core.Teams;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Security;

namespace Squidex.Domain.Apps.Entities;

public interface IAppProvider
{
    Task<(App?, Schema?)> GetAppWithSchemaAsync(DomainId appId, DomainId id, bool canCache = false,
        CancellationToken ct = default);

    Task<Team?> GetTeamAsync(DomainId teamId,
        CancellationToken ct = default);

    Task<Team?> GetTeamByAuthDomainAsync(string authDomain,
        CancellationToken ct = default);

    Task<List<Team>> GetUserTeamsAsync(string userId,
        CancellationToken ct = default);

    Task<App?> GetAppAsync(DomainId appId, bool canCache = false,
        CancellationToken ct = default);

    Task<App?> GetAppAsync(string appName, bool canCache = false,
        CancellationToken ct = default);

    Task<List<App>> GetUserAppsAsync(string userId, PermissionSet permissions,
        CancellationToken ct = default);

    Task<List<App>> GetTeamAppsAsync(DomainId teamId,
        CancellationToken ct = default);

    Task<Schema?> GetSchemaAsync(DomainId appId, DomainId id, bool canCache = false,
        CancellationToken ct = default);

    Task<Schema?> GetSchemaAsync(DomainId appId, string name, bool canCache = false,
        CancellationToken ct = default);

    Task<List<Schema>> GetSchemasAsync(DomainId appId,
        CancellationToken ct = default);

    Task<List<Rule>> GetRulesAsync(DomainId appId,
        CancellationToken ct = default);

    Task<Rule?> GetRuleAsync(DomainId appId, DomainId id,
        CancellationToken ct = default);

    void RegisterAppForLocalContext(DomainId appId, App app);
}
