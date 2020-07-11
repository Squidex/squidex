// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Squidex.Domain.Apps.Entities.Apps;
using Squidex.Domain.Apps.Entities.Apps.Indexes;
using Squidex.Domain.Apps.Entities.Rules;
using Squidex.Domain.Apps.Entities.Rules.Indexes;
using Squidex.Domain.Apps.Entities.Schemas;
using Squidex.Domain.Apps.Entities.Schemas.Indexes;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Caching;
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

        public async Task<(IAppEntity?, ISchemaEntity?)> GetAppWithSchemaAsync(Guid appId, Guid id, bool canCache = false)
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

        public Task<IAppEntity?> GetAppAsync(Guid appId, bool canCache = false)
        {
            return localCache.GetOrCreate(AppCacheKey(appId), async () =>
            {
                var app = await indexForApps.GetAppAsync(appId, canCache);

                if (app != null)
                {
                    localCache.Add(AppCacheKey(app.Name), app);
                }

                return app;
            });
        }

        public Task<IAppEntity?> GetAppAsync(string appName, bool canCache = false)
        {
            return localCache.GetOrCreate(AppCacheKey(appName), async () =>
            {
                var app = await indexForApps.GetAppByNameAsync(appName, canCache);

                if (app != null)
                {
                    localCache.Add(AppCacheKey(app.Id), app);
                }

                return app;
            });
        }

        public Task<ISchemaEntity?> GetSchemaAsync(Guid appId, string name, bool canCache = false)
        {
            return localCache.GetOrCreate(SchemaCacheKey(appId, name), async () =>
            {
                var schema = await indexSchemas.GetSchemaByNameAsync(appId, name, canCache);

                if (schema != null)
                {
                    localCache.Add(SchemaCacheKey(appId, schema.Id), schema);
                }

                return schema;
            });
        }

        public Task<ISchemaEntity?> GetSchemaAsync(Guid appId, Guid id, bool allowDeleted = false, bool canCache = false)
        {
            return localCache.GetOrCreate(SchemaCacheKey(appId, id), async () =>
            {
                var schema = await indexSchemas.GetSchemaAsync(appId, id, canCache);

                if (schema != null)
                {
                    localCache.Add(SchemaCacheKey(appId, schema.SchemaDef.Name), schema);
                }

                return schema;
            });
        }

        public Task<List<IAppEntity>> GetUserAppsAsync(string userId, PermissionSet permissions)
        {
            return localCache.GetOrCreate($"GetUserApps({userId})", async () =>
            {
                return await indexForApps.GetAppsForUserAsync(userId, permissions);
            });
        }

        public Task<List<ISchemaEntity>> GetSchemasAsync(Guid appId)
        {
            return localCache.GetOrCreate($"GetSchemasAsync({appId})", async () =>
            {
                return await indexSchemas.GetSchemasAsync(appId);
            });
        }

        public Task<List<IRuleEntity>> GetRulesAsync(Guid appId)
        {
            return localCache.GetOrCreate($"GetRulesAsync({appId})", async () =>
            {
                return await indexRules.GetRulesAsync(appId);
            });
        }

        private static string AppCacheKey(Guid appId)
        {
            return $"APPS_ID_{appId}";
        }

        private static string AppCacheKey(string appName)
        {
            return $"APPS_NAME_{appName}";
        }

        private static string SchemaCacheKey(Guid appId, Guid id)
        {
            return $"SCHEMAS_ID_{appId}_{id}";
        }

        private static string SchemaCacheKey(Guid appId, string name)
        {
            return $"SCHEMAS_NAME_{appId}_{name}";
        }
    }
}
