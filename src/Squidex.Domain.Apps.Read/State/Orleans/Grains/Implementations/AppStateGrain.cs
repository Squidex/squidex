// ==========================================================================
//  AppStateGrain.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Orleans;
using Orleans.Providers;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Domain.Apps.Read.Apps;
using Squidex.Domain.Apps.Read.Rules;
using Squidex.Domain.Apps.Read.Schemas;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Read.State.Orleans.Grains.Implementations
{
    [StorageProvider(ProviderName = "Default")]
    public sealed class AppStateGrain : Grain<AppStateGrainState>, IAppStateGrain
    {
        private readonly FieldRegistry fieldRegistry;

        public AppStateGrain(FieldRegistry fieldRegistry)
        {
            Guard.NotNull(fieldRegistry, nameof(fieldRegistry));

            this.fieldRegistry = fieldRegistry;
        }

        public Task<IAppEntity> GetAppAsync()
        {
            var value = State.App;

            return Task.FromResult<IAppEntity>(value);
        }

        public Task<List<IRuleEntity>> GetRulesAsync()
        {
            var value = State.Rules.Values.OfType<IRuleEntity>().ToList();

            return Task.FromResult(value);
        }

        public Task<List<ISchemaEntity>> GetSchemasAsync()
        {
            var value = State.Schemas.Values.Where(x => !x.IsDeleted).OfType<ISchemaEntity>().ToList();

            return Task.FromResult(value);
        }

        public Task<ISchemaEntity> GetSchemaAsync(Guid id, bool provideDeleted = false)
        {
            var value = State.Schemas.Values.FirstOrDefault(x => x.Id == id && (!x.IsDeleted || provideDeleted));

            return Task.FromResult<ISchemaEntity>(value);
        }

        public Task<ISchemaEntity> GetSchemaAsync(string name, bool provideDeleted = false)
        {
            var value = State.Schemas.Values.FirstOrDefault(x => string.Equals(x.Name, name, StringComparison.OrdinalIgnoreCase) && (!x.IsDeleted || provideDeleted));

            return Task.FromResult<ISchemaEntity>(value);
        }

        public Task HandleAsync(EventMessage message)
        {
            State.Apply(message.Event, fieldRegistry);

            return WriteStateAsync();
        }
    }
}
