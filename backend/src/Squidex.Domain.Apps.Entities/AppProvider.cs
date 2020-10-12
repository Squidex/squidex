// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
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
            Guard.NotNull(indexForApps, nameof(indexForApps));
            Guard.NotNull(indexRules, nameof(indexRules));
            Guard.NotNull(indexSchemas, nameof(indexSchemas));
            Guard.NotNull(localCache, nameof(localCache));

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

            var schema = await GetSchemaAsync(appId, id, false, canCache);

            if (schema == null)
            {
                return (null, null);
            }

            return (app, schema);
        }

        public async Task<IAppEntity?> GetAppAsync(DomainId appId, bool canCache = false)
        {
            var app = await localCache.GetOrCreateAsync(AppCacheKey(appId), () =>
            {
                return indexForApps.GetAppAsync(appId, canCache);
            });

            if (app != null)
            {
                localCache.Add(AppCacheKey(app.Id), app);
            }

            return app?.IsArchived == true ? null : app;
        }

        public async Task<IAppEntity?> GetAppAsync(string appName, bool canCache = false)
        {
            var app = await localCache.GetOrCreateAsync(AppCacheKey(appName), () =>
            {
                return indexForApps.GetAppByNameAsync(appName, canCache);
            });

            if (app != null)
            {
                localCache.Add(AppCacheKey(app.Id), app);
            }

            return app?.IsArchived == true ? null : app;
        }

        public async Task<ISchemaEntity?> GetSchemaAsync(DomainId appId, string name, bool canCache = false)
        {
            var schema = await localCache.GetOrCreateAsync(SchemaCacheKey(appId, name), () =>
            {
                return indexSchemas.GetSchemaByNameAsync(appId, name, canCache);
            });

            if (schema != null)
            {
                localCache.Add(SchemaCacheKey(appId, schema.Id), schema);
            }

            return schema?.IsDeleted == true ? null : schema;
        }

        public async Task<ISchemaEntity?> GetSchemaAsync(DomainId appId, DomainId id, bool allowDeleted = false, bool canCache = false)
        {
            var schema = await localCache.GetOrCreateAsync(SchemaCacheKey(appId, id), () =>
            {
                return indexSchemas.GetSchemaAsync(appId, id, canCache);
            });

            if (schema != null)
            {
                localCache.Add(SchemaCacheKey(appId, schema.Id), schema);
            }

            return schema?.IsDeleted == true && !allowDeleted ? null : schema;
        }

        public async Task<List<IAppEntity>> GetUserAppsAsync(string userId, PermissionSet permissions)
        {
            var apps = await localCache.GetOrCreateAsync($"GetUserApps({userId})", () =>
            {
                return indexForApps.GetAppsForUserAsync(userId, permissions);
            });

            return apps.Where(x => !x.IsArchived).ToList();
        }

        public async Task<List<ISchemaEntity>> GetSchemasAsync(DomainId appId)
        {
            var schemas = await localCache.GetOrCreateAsync($"GetSchemasAsync({appId})", () =>
            {
                return indexSchemas.GetSchemasAsync(appId);
            });

            return schemas.Where(x => !x.IsDeleted).ToList();
        }

        public async Task<List<IRuleEntity>> GetRulesAsync(DomainId appId)
        {
            var rules = await localCache.GetOrCreateAsync($"GetRulesAsync({appId})", () =>
            {
                return indexRules.GetRulesAsync(appId);
            });

            return rules.Where(x => !x.IsDeleted).ToList();
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
