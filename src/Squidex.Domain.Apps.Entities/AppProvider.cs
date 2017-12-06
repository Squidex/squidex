// ==========================================================================
//  AppProvider.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Squidex.Domain.Apps.Entities.Apps;
using Squidex.Domain.Apps.Entities.Rules;
using Squidex.Domain.Apps.Entities.Schemas;
using Squidex.Infrastructure;
using Squidex.Infrastructure.States;

namespace Squidex.Domain.Apps.Entities
{
    public sealed class AppProvider : IAppProvider
    {
        private readonly IStateFactory factory;

        public AppProvider(IStateFactory factory)
        {
            Guard.NotNull(factory, nameof(factory));

            this.factory = factory;
        }

        public Task<IAppEntity> GetAppAsync(string appName)
        {
            return null;
        }

        public Task<(IAppEntity, ISchemaEntity)> GetAppWithSchemaAsync(string appName, Guid id)
        {
            return null;
        }

        public Task<List<IRuleEntity>> GetRulesAsync(string appName)
        {
            return null;
        }

        public Task<ISchemaEntity> GetSchemaAsync(string appName, Guid id, bool provideDeleted = false)
        {
            return null;
        }

        public Task<ISchemaEntity> GetSchemaAsync(string appName, string name, bool provideDeleted = false)
        {
            return null;
        }

        public Task<List<ISchemaEntity>> GetSchemasAsync(string appName)
        {
            return null;
        }

        public Task<List<IAppEntity>> GetUserApps(string userId)
        {
            return null;
        }
    }
}
