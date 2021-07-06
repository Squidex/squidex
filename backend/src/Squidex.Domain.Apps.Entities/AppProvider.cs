// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
        private readonly IRulesIndex indexRules;
        private readonly ISchemasIndex indexSchemas;

        public AppProvider(ILocalCache localCache, IAppsIndex indexForApps, IRulesIndex indexRules, ISchemasIndex indexSchemas)
        {
            this.localCache = localCache;
            this.indexForApps = indexForApps;
            this.indexRules = indexRules;
            this.indexSchemas = indexSchemas;
        }

        public async Task<(IAppEntity?, ISchemaEntity?)> GetAppWithSchemaAsync(DomainId appId, DomainId id, bool canCache = false)
        {
            var app = await GetAppAsync(appId, canCache);

            if (app == null)
            {
                return (null, null);
            }

            var schema = await GetSchemaAsync(appId, id, canCache);

            if (schema == null)
            {
                return (null, null);
            }

            return (app, schema);
        }

        public async Task<IAppEntity?> GetAppAsync(DomainId appId, bool canCache = false)
        {
            var cacheKey = AppCacheKey(appId);

            if (localCache.TryGetValue(cacheKey, out var cached) && cached is IAppEntity found)
            {
                return found;
            }

            var app = await indexForApps.GetAppAsync(appId, canCache);

            if (app != null)
            {
                localCache.Add(cacheKey, app);
                localCache.Add(AppCacheKey(app.Name), app);
            }

            return app;
        }

        public async Task<IAppEntity?> GetAppAsync(string appName, bool canCache = false)
        {
            var cacheKey = AppCacheKey(appName);

            if (localCache.TryGetValue(cacheKey, out var cached) && cached is IAppEntity found)
            {
                return found;
            }

            var app = await indexForApps.GetAppByNameAsync(appName, canCache);

            if (app != null)
            {
                localCache.Add(cacheKey, app);
                localCache.Add(AppCacheKey(app.Id), app);
            }

            return app;
        }

        public async Task<ISchemaEntity?> GetSchemaAsync(DomainId appId, string name, bool canCache = false)
        {
            var cacheKey = SchemaCacheKey(appId, name);

            if (localCache.TryGetValue(cacheKey, out var cached) && cached is ISchemaEntity found)
            {
                return found;
            }

            var schema = await indexSchemas.GetSchemaByNameAsync(appId, name, canCache);

            if (schema != null)
            {
                localCache.Add(cacheKey, schema);
                localCache.Add(SchemaCacheKey(appId, schema.Id), schema);
            }

            return schema;
        }

        public async Task<ISchemaEntity?> GetSchemaAsync(DomainId appId, DomainId id, bool canCache = false)
        {
            var cacheKey = SchemaCacheKey(appId, id);

            if (localCache.TryGetValue(cacheKey, out var cached) && cached is ISchemaEntity found)
            {
                return found;
            }

            var schema = await indexSchemas.GetSchemaAsync(appId, id, canCache);

            if (schema != null)
            {
                localCache.Add(cacheKey, schema);
                localCache.Add(SchemaCacheKey(appId, schema.SchemaDef.Name), schema);
            }

            return schema;
        }

        public async Task<List<IAppEntity>> GetUserAppsAsync(string userId, PermissionSet permissions)
        {
            var apps = await localCache.GetOrCreateAsync($"GetUserApps({userId})", () =>
            {
                return indexForApps.GetAppsForUserAsync(userId, permissions);
            });

            return apps;
        }

        public async Task<List<ISchemaEntity>> GetSchemasAsync(DomainId appId)
        {
            var schemas = await localCache.GetOrCreateAsync($"GetSchemasAsync({appId})", () =>
            {
                return indexSchemas.GetSchemasAsync(appId);
            });

            foreach (var schema in schemas)
            {
                localCache.Add(SchemaCacheKey(appId, schema.Id), schema);
                localCache.Add(SchemaCacheKey(appId, schema.SchemaDef.Name), schema);
            }

            return schemas;
        }

        public async Task<List<IRuleEntity>> GetRulesAsync(DomainId appId)
        {
            var rules = await localCache.GetOrCreateAsync($"GetRulesAsync({appId})", () =>
            {
                return indexRules.GetRulesAsync(appId);
            });

            return rules.ToList();
        }

        public async Task<IRuleEntity?> GetRuleAsync(DomainId appId, DomainId id)
        {
            var rules = await GetRulesAsync(appId);

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
