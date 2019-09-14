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

        public Task<(IAppEntity, ISchemaEntity)> GetAppWithSchemaAsync(Guid appId, Guid id)
        {
            return localCache.GetOrCreateAsync($"GetAppWithSchemaAsync({appId}, {id})", async () =>
            {
                var app = await GetAppAsync(appId);

                if (app == null)
                {
                    return (null, null);
                }

                var schema = await GetSchemaAsync(appId, id, false);

                if (schema == null)
                {
                    return (null, null);
                }

                return (app, schema);
            });
        }

        public Task<IAppEntity> GetAppAsync(Guid appId)
        {
            return localCache.GetOrCreateAsync($"GetAppAsync({appId})", async () =>
            {
                return await indexForApps.GetAppAsync(appId);
            });
        }

        public Task<IAppEntity> GetAppAsync(string appName)
        {
            return localCache.GetOrCreateAsync($"GetAppAsync({appName})", async () =>
            {
                return await indexForApps.GetAppAsync(appName);
            });
        }

        public Task<List<IAppEntity>> GetUserAppsAsync(string userId, PermissionSet permissions)
        {
            return localCache.GetOrCreateAsync($"GetUserApps({userId})", async () =>
            {
                return await indexForApps.GetAppsForUserAsync(userId, permissions);
            });
        }

        public Task<ISchemaEntity> GetSchemaAsync(Guid appId, string name)
        {
            return localCache.GetOrCreateAsync($"GetSchemaAsync({appId}, {name})", async () =>
            {
                return await indexSchemas.GetSchemaAsync(appId, name);
            });
        }

        public Task<ISchemaEntity> GetSchemaAsync(Guid appId, Guid id, bool allowDeleted = false)
        {
            return localCache.GetOrCreateAsync($"GetSchemaAsync({appId}, {id}, {allowDeleted})", async () =>
            {
                return await indexSchemas.GetSchemaAsync(appId, id, allowDeleted);
            });
        }

        public Task<List<ISchemaEntity>> GetSchemasAsync(Guid appId)
        {
            return localCache.GetOrCreateAsync($"GetSchemasAsync({appId})", async () =>
            {
                return await indexSchemas.GetSchemasAsync(appId);
            });
        }

        public Task<List<IRuleEntity>> GetRulesAsync(Guid appId)
        {
            return localCache.GetOrCreateAsync($"GetRulesAsync({appId})", async () =>
            {
                return await indexRules.GetRulesAsync(appId);
            });
        }
    }
}
