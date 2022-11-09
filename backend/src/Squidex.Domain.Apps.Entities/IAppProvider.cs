// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Entities.Apps;
using Squidex.Domain.Apps.Entities.Rules;
using Squidex.Domain.Apps.Entities.Schemas;
using Squidex.Domain.Apps.Entities.Teams;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Security;

namespace Squidex.Domain.Apps.Entities;

public interface IAppProvider
{
    Task<(IAppEntity?, ISchemaEntity?)> GetAppWithSchemaAsync(DomainId appId, DomainId id, bool canCache = false,
        CancellationToken ct = default);

    Task<ITeamEntity?> GetTeamAsync(DomainId teamId,
        CancellationToken ct = default);

    Task<List<ITeamEntity>> GetUserTeamsAsync(string userId,
        CancellationToken ct = default);

    Task<IAppEntity?> GetAppAsync(DomainId appId, bool canCache = false,
        CancellationToken ct = default);

    Task<IAppEntity?> GetAppAsync(string appName, bool canCache = false,
        CancellationToken ct = default);

    Task<List<IAppEntity>> GetUserAppsAsync(string userId, PermissionSet permissions,
        CancellationToken ct = default);

    Task<List<IAppEntity>> GetTeamAppsAsync(DomainId teamId,
        CancellationToken ct = default);

    Task<ISchemaEntity?> GetSchemaAsync(DomainId appId, DomainId id, bool canCache = false,
        CancellationToken ct = default);

    Task<ISchemaEntity?> GetSchemaAsync(DomainId appId, string name, bool canCache = false,
        CancellationToken ct = default);

    Task<List<ISchemaEntity>> GetSchemasAsync(DomainId appId,
        CancellationToken ct = default);

    Task<List<IRuleEntity>> GetRulesAsync(DomainId appId,
        CancellationToken ct = default);

    Task<IRuleEntity?> GetRuleAsync(DomainId appId, DomainId id,
        CancellationToken ct = default);
}
