// ==========================================================================
//  AppProvider.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Squidex.Domain.Apps.Read.Apps;
using Squidex.Domain.Apps.Read.Rules;
using Squidex.Domain.Apps.Read.Schemas;
using Squidex.Domain.Apps.Read.State.Grains;
using Squidex.Infrastructure;
using Squidex.Infrastructure.States;

namespace Squidex.Domain.Apps.Read.State
{
    public sealed class AppProvider : IAppProvider
    {
        private readonly IStateFactory factory;

        public AppProvider(IStateFactory factory)
        {
            Guard.NotNull(factory, nameof(factory));

            this.factory = factory;
        }

        public async Task<IAppEntity> GetAppAsync(string appName)
        {
            var app = await factory.GetSynchronizedAsync<AppStateGrain>(appName);

            return await app.GetAppAsync();
        }

        public async Task<(IAppEntity, ISchemaEntity)> GetAppWithSchemaAsync(string appName, Guid id)
        {
            var app = await factory.GetSynchronizedAsync<AppStateGrain>(appName);

            return await app.GetAppWithSchemaAsync(id);
        }

        public async Task<List<IRuleEntity>> GetRulesAsync(string appName)
        {
            var app = await factory.GetSynchronizedAsync<AppStateGrain>(appName);

            return await app.GetRulesAsync();
        }

        public async Task<ISchemaEntity> GetSchemaAsync(string appName, Guid id, bool provideDeleted = false)
        {
            var app = await factory.GetSynchronizedAsync<AppStateGrain>(appName);

            return await app.GetSchemaAsync(id, provideDeleted);
        }

        public async Task<ISchemaEntity> GetSchemaAsync(string appName, string name, bool provideDeleted = false)
        {
            var app = await factory.GetSynchronizedAsync<AppStateGrain>(appName);

            return await app.GetSchemaAsync(name, provideDeleted);
        }

        public async Task<List<ISchemaEntity>> GetSchemasAsync(string appName)
        {
            var app = await factory.GetSynchronizedAsync<AppStateGrain>(appName);

            return await app.GetSchemasAsync();
        }

        public async Task<List<IAppEntity>> GetUserApps(string userId)
        {
            var appUser = await factory.GetSynchronizedAsync<AppUserGrain>(userId);
            var appNames = await appUser.GetAppNamesAsync();

            var tasks = appNames.Select(x => GetAppAsync(x));

            var apps = await Task.WhenAll(tasks);

            return apps.Where(a => a != null && a.Contributors.ContainsKey(userId)).ToList();
        }
    }
}
