﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Caching;
using Squidex.Domain.Apps.Entities.Apps;
using Squidex.Domain.Apps.Entities.Apps.Indexes;
using Squidex.Domain.Apps.Entities.Rules;
using Squidex.Domain.Apps.Entities.Rules.Indexes;
using Squidex.Domain.Apps.Entities.Schemas;
using Squidex.Domain.Apps.Entities.Schemas.Indexes;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Security;

namespace Squidex.Domain.Apps.Entities
{
    public sealed class AppProvider : IAppProvider
    {
        private readonly ILocalCache localCache;
        private readonly IAppsIndex indexForApps;
        private readonly IRulesIndex indexForRules;
        private readonly ISchemasIndex indexForSchemas;

        public AppProvider(IAppsIndex indexForApps, IRulesIndex indexForRules, ISchemasIndex indexForSchemas,
            ILocalCache localCache)
        {
            this.localCache = localCache;
            this.indexForApps = indexForApps;
            this.indexForRules = indexForRules;
            this.indexForSchemas = indexForSchemas;
        }

        public async Task<(IAppEntity?, ISchemaEntity?)> GetAppWithSchemaAsync(DomainId appId, DomainId id, bool canCache = false,
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

        public async Task<IAppEntity?> GetAppAsync(DomainId appId, bool canCache = false,
            CancellationToken ct = default)
        {
            var cacheKey = AppCacheKey(appId);

            var app = await localCache.GetOrCreate(cacheKey, () =>
            {
                return indexForApps.GetAppAsync(appId, canCache, ct);
            });

            if (app != null)
            {
                localCache.Add(AppCacheKey(app.Name), app);
            }

            return app;
        }

        public async Task<IAppEntity?> GetAppAsync(string appName, bool canCache = false,
            CancellationToken ct = default)
        {
            var cacheKey = AppCacheKey(appName);

            var app = await localCache.GetOrCreate(cacheKey, () =>
            {
                return indexForApps.GetAppAsync(appName, canCache, ct);
            });

            if (app != null)
            {
                localCache.Add(AppCacheKey(app.Id), app);
            }

            return app;
        }

        public async Task<ISchemaEntity?> GetSchemaAsync(DomainId appId, string name, bool canCache = false,
            CancellationToken ct = default)
        {
            var cacheKey = SchemaCacheKey(appId, name);

            var schema = await localCache.GetOrCreate(cacheKey, () =>
            {
                return indexForSchemas.GetSchemaAsync(appId, name, canCache, ct);
            });

            if (schema != null)
            {
                localCache.Add(SchemaCacheKey(appId, schema.Id), schema);
            }

            return schema;
        }

        public async Task<ISchemaEntity?> GetSchemaAsync(DomainId appId, DomainId id, bool canCache = false,
            CancellationToken ct = default)
        {
            var cacheKey = SchemaCacheKey(appId, id);

            var schema = await localCache.GetOrCreate(cacheKey, () =>
            {
                return indexForSchemas.GetSchemaAsync(appId, id, canCache, ct);
            });

            if (schema != null)
            {
                localCache.Add(SchemaCacheKey(appId, schema.SchemaDef.Name), schema);
            }

            return schema;
        }

        public async Task<List<IAppEntity>> GetUserAppsAsync(string userId, PermissionSet permissions,
            CancellationToken ct = default)
        {
            var apps = await localCache.GetOrCreate($"GetUserApps({userId})", () =>
            {
                return indexForApps.GetAppsForUserAsync(userId, permissions, ct);
            });

            return apps;
        }

        public async Task<List<ISchemaEntity>> GetSchemasAsync(DomainId appId,
            CancellationToken ct = default)
        {
            var schemas = await localCache.GetOrCreate($"GetSchemasAsync({appId})", () =>
            {
                return indexForSchemas.GetSchemasAsync(appId, ct);
            });

            foreach (var schema in schemas)
            {
                localCache.Add(SchemaCacheKey(appId, schema.Id), schema);
                localCache.Add(SchemaCacheKey(appId, schema.SchemaDef.Name), schema);
            }

            return schemas;
        }

        public async Task<List<IRuleEntity>> GetRulesAsync(DomainId appId,
            CancellationToken ct = default)
        {
            var rules = await localCache.GetOrCreate($"GetRulesAsync({appId})", () =>
            {
                return indexForRules.GetRulesAsync(appId, ct);
            });

            return rules.ToList();
        }

        public async Task<IRuleEntity?> GetRuleAsync(DomainId appId, DomainId id,
            CancellationToken ct = default)
        {
            var rules = await GetRulesAsync(appId, ct);

            return rules.Find(x => x.Id == id);
        }

        private static string AppCacheKey(DomainId appId)
        {
            return $"APPS_ID_{appId}";
        }

        private static string AppCacheKey(string appName)
        {
            return $"APPS_NAME_{appName}";
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
}
