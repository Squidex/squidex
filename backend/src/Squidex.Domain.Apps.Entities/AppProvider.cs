// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Caching;
using Squidex.Domain.Apps.Core.Apps;
using Squidex.Domain.Apps.Core.Rules;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Domain.Apps.Core.Teams;
using Squidex.Domain.Apps.Entities.Apps.Indexes;
using Squidex.Domain.Apps.Entities.Rules.Indexes;
using Squidex.Domain.Apps.Entities.Schemas.Indexes;
using Squidex.Domain.Apps.Entities.Teams.Indexes;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Security;

namespace Squidex.Domain.Apps.Entities;

public sealed class AppProvider : IAppProvider
{
    private readonly ILocalCache localCache;
    private readonly IAppsIndex indexForApps;
    private readonly IRulesIndex indexForRules;
    private readonly ISchemasIndex indexForSchemas;
    private readonly ITeamsIndex indexForTeams;

    public AppProvider(IAppsIndex indexForApps, IRulesIndex indexForRules, ISchemasIndex indexForSchemas, ITeamsIndex indexForTeams,
        ILocalCache localCache)
    {
        this.localCache = localCache;
        this.indexForApps = indexForApps;
        this.indexForRules = indexForRules;
        this.indexForSchemas = indexForSchemas;
        this.indexForTeams = indexForTeams;
    }

    public async Task<(App?, Schema?)> GetAppWithSchemaAsync(DomainId appId, DomainId id, bool canCache = false,
        CancellationToken ct = default)
    {
        var app = await GetAppAsync(appId, canCache, ct);

        if (app == null)
        {
            return (null, null);
        }

        var schema = await GetSchemaAsync(appId, id, canCache, ct);

        if (schema == null)
        {
            return (null, null);
        }

        return (app, schema);
    }

    public async Task<App?> GetAppAsync(DomainId appId, bool canCache = false,
        CancellationToken ct = default)
    {
        var cacheKey = AppCacheKey(appId);

        var app = await GetOrCreate(cacheKey, () =>
        {
            return indexForApps.GetAppAsync(appId, canCache, ct);
        });

        if (app != null)
        {
            localCache.Add(AppCacheKey(app.Name), app);
        }

        return app;
    }

    public async Task<App?> GetAppAsync(string appName, bool canCache = false,
        CancellationToken ct = default)
    {
        var cacheKey = AppCacheKey(appName);

        var app = await GetOrCreate(cacheKey, () =>
        {
            return indexForApps.GetAppAsync(appName, canCache, ct);
        });

        if (app != null)
        {
            localCache.Add(AppCacheKey(app.Id), app);
        }

        return app;
    }

    public async Task<Team?> GetTeamAsync(DomainId teamId,
        CancellationToken ct = default)
    {
        var cacheKey = TeamCacheKey(teamId);

        var team = await GetOrCreate(cacheKey, () =>
        {
            return indexForTeams.GetTeamAsync(teamId, ct);
        });

        return team;
    }

    public async Task<Team?> GetTeamByAuthDomainAsync(string authDomain,
        CancellationToken ct = default)
    {
        var cacheKey = TeamCacheKey(authDomain);

        var team = await GetOrCreate(cacheKey, () =>
        {
            return indexForTeams.GetTeamByAuthDomainAsync(authDomain, ct);
        });

        return team;
    }

    public async Task<Schema?> GetSchemaAsync(DomainId appId, string name, bool canCache = false,
        CancellationToken ct = default)
    {
        var cacheKey = SchemaCacheKey(appId, name);

        var schema = await GetOrCreate(cacheKey, () =>
        {
            return indexForSchemas.GetSchemaAsync(appId, name, canCache, ct);
        });

        if (schema != null)
        {
            localCache.Add(SchemaCacheKey(appId, schema.Id), schema);
        }

        return schema;
    }

    public async Task<Schema?> GetSchemaAsync(DomainId appId, DomainId id, bool canCache = false,
        CancellationToken ct = default)
    {
        var cacheKey = SchemaCacheKey(appId, id);

        var schema = await GetOrCreate(cacheKey, () =>
        {
            return indexForSchemas.GetSchemaAsync(appId, id, canCache, ct);
        });

        if (schema != null)
        {
            localCache.Add(SchemaCacheKey(appId, schema.Name), schema);
        }

        return schema;
    }

    public async Task<List<App>> GetUserAppsAsync(string userId, PermissionSet permissions,
        CancellationToken ct = default)
    {
        var apps = await GetOrCreate($"GetUserApps({userId})", () =>
        {
            return indexForApps.GetAppsForUserAsync(userId, permissions, ct)!;
        });

        return apps?.ToList() ?? [];
    }

    public async Task<List<App>> GetTeamAppsAsync(DomainId teamId,
        CancellationToken ct = default)
    {
        var apps = await GetOrCreate($"GetTeamApps({teamId})", () =>
        {
            return indexForApps.GetAppsForTeamAsync(teamId, ct)!;
        });

        return apps?.ToList() ?? [];
    }

    public async Task<List<Team>> GetUserTeamsAsync(string userId, CancellationToken ct = default)
    {
        var teams = await GetOrCreate($"GetUserTeams({userId})", () =>
        {
            return indexForTeams.GetTeamsAsync(userId, ct)!;
        });

        return teams?.ToList() ?? [];
    }

    public async Task<List<Schema>> GetSchemasAsync(DomainId appId,
        CancellationToken ct = default)
    {
        var schemas = await GetOrCreate($"GetSchemasAsync({appId})", () =>
        {
            return indexForSchemas.GetSchemasAsync(appId, ct)!;
        });

        if (schemas != null)
        {
            foreach (var schema in schemas)
            {
                localCache.Add(SchemaCacheKey(appId, schema.Id), schema);
                localCache.Add(SchemaCacheKey(appId, schema.Name), schema);
            }
        }

        return schemas?.ToList() ?? [];
    }

    public async Task<List<Rule>> GetRulesAsync(DomainId appId,
        CancellationToken ct = default)
    {
        var rules = await GetOrCreate($"GetRulesAsync({appId})", () =>
        {
            return indexForRules.GetRulesAsync(appId, ct)!;
        });

        return rules?.ToList() ?? [];
    }

    public async Task<Rule?> GetRuleAsync(DomainId appId, DomainId id,
        CancellationToken ct = default)
    {
        var rules = await GetRulesAsync(appId, ct);

        return rules.Find(x => x.Id == id);
    }

    public void RegisterAppForLocalContext(DomainId appId, App app)
    {
        localCache.Add(AppCacheKey(appId), app);
    }

    public async Task<T?> GetOrCreate<T>(object key, Func<Task<T?>> creator) where T : class
    {
        if (localCache.TryGetValue(key, out var value))
        {
            switch (value)
            {
                case T typed:
                    return typed;
                case Task<T?> typedTask:
                    return await typedTask;
                default:
                    return null;
            }
        }

        var result = creator();

        localCache.Add(key, result);

        return await result;
    }

    private static string AppCacheKey(DomainId appId)
    {
        return $"APPS_ID_{appId}";
    }

    private static string AppCacheKey(string appName)
    {
        return $"APPS_NAME_{appName}";
    }

    private static string TeamCacheKey(DomainId teamId)
    {
        return $"TEAMS_ID_{teamId}";
    }

    private static string TeamCacheKey(string authDomain)
    {
        return $"TEAMS_DOMAIN_{authDomain}";
    }

    private static string SchemaCacheKey(DomainId appId, DomainId id)
    {
        return $"SCHEMAS_ID_{appId}_{id}";
    }

    private static string SchemaCacheKey(DomainId appId, string name)
    {
        return $"SCHEMAS_NAME_{appId}_{name}";
    }
}
