// ==========================================================================
//  AppStateGrain.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Orleans;
using Orleans.Concurrency;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Domain.Apps.Read.Apps;
using Squidex.Domain.Apps.Read.Rules;
using Squidex.Domain.Apps.Read.Schemas;
using Squidex.Infrastructure;
using Squidex.Infrastructure.CQRS.Events;

namespace Squidex.Domain.Apps.Read.State.Orleans.Grains.Implementations
{
    public sealed class AppStateGrain : Grain<AppStateGrainState>, IAppStateGrain
    {
        private readonly FieldRegistry fieldRegistry;

        public AppStateGrain(FieldRegistry fieldRegistry)
        {
            Guard.NotNull(fieldRegistry, nameof(fieldRegistry));

            this.fieldRegistry = fieldRegistry;
        }

        public override Task OnActivateAsync()
        {
            State.SetRegistry(fieldRegistry);

            return base.OnActivateAsync();
        }

        public Task<Immutable<(IAppEntity, ISchemaEntity)>> GetAppWithSchemaAsync(Guid id)
        {
            var schema = State.FindSchema(x => x.Id == id && !x.IsDeleted);

            return Task.FromResult((State.GetApp(), schema).AsImmutable());
        }

        public Task<Immutable<IAppEntity>> GetAppAsync()
        {
            var value = State.GetApp();

            return Task.FromResult(value.AsImmutable());
        }

        public Task<Immutable<List<IRuleEntity>>> GetRulesAsync()
        {
            var value = State.FindRules();

            return Task.FromResult(value.AsImmutable());
        }

        public Task<Immutable<List<ISchemaEntity>>> GetSchemasAsync()
        {
            var value = State.FindSchemas(x => !x.IsDeleted);

            return Task.FromResult(value.AsImmutable());
        }

        public Task<Immutable<ISchemaEntity>> GetSchemaAsync(Guid id, bool provideDeleted = false)
        {
            var value = State.FindSchema(x => x.Id == id && (!x.IsDeleted || provideDeleted));

            return Task.FromResult(value.AsImmutable());
        }

        public Task<Immutable<ISchemaEntity>> GetSchemaAsync(string name, bool provideDeleted = false)
        {
            var value = State.FindSchema(x => string.Equals(x.Name, name, StringComparison.OrdinalIgnoreCase) && (!x.IsDeleted || provideDeleted));

            return Task.FromResult(value.AsImmutable());
        }

        public Task HandleAsync(Immutable<Envelope<IEvent>> message)
        {
            State.Apply(message.Value);

            return WriteStateAsync();
        }
    }
}
