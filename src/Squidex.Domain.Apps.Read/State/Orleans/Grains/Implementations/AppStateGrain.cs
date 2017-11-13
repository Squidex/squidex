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
using Newtonsoft.Json;
using Orleans;
using Orleans.Providers;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Domain.Apps.Read.Apps;
using Squidex.Domain.Apps.Read.Rules;
using Squidex.Domain.Apps.Read.Schemas;
using Squidex.Infrastructure;
using Squidex.Infrastructure.CQRS.Events;
using Squidex.Infrastructure.Json.Orleans;

namespace Squidex.Domain.Apps.Read.State.Orleans.Grains.Implementations
{
    [StorageProvider(ProviderName = "Default")]
    public sealed class AppStateGrain : Grain<JsonState<AppStateGrainState>>, IAppStateGrain
    {
        private readonly FieldRegistry fieldRegistry;
        private readonly JsonSerializer serializer;

        public AppStateGrain(FieldRegistry fieldRegistry, JsonSerializer serializer)
        {
            Guard.NotNull(fieldRegistry, nameof(fieldRegistry));
            Guard.NotNull(serializer, nameof(serializer));

            this.fieldRegistry = fieldRegistry;
            this.serializer = serializer;
        }

        public override Task OnActivateAsync()
        {
            State.SetSerializer(serializer);

            return base.OnActivateAsync();
        }

        public Task<J<(IAppEntity, ISchemaEntity)>> GetAppWithSchemaAsync(Guid id)
        {
            var schema = State.Value.FindSchema(x => x.Id == id && !x.IsDeleted);

            return J<(IAppEntity AppEntity, ISchemaEntity SchemaEntity)>.AsTask((State.Value.GetApp(), schema));
        }

        public Task<J<IAppEntity>> GetAppAsync()
        {
            var value = State.Value.GetApp();

            return J<IAppEntity>.AsTask(value);
        }

        public Task<J<List<IRuleEntity>>> GetRulesAsync()
        {
            var value = State.Value.FindRules();

            return J<List<IRuleEntity>>.AsTask(value);
        }

        public Task<J<List<ISchemaEntity>>> GetSchemasAsync()
        {
            var value = State.Value.FindSchemas(x => !x.IsDeleted);

            return J<List<ISchemaEntity>>.AsTask(value);
        }

        public Task<J<ISchemaEntity>> GetSchemaAsync(Guid id, bool provideDeleted = false)
        {
            var value = State.Value.FindSchema(x => x.Id == id && (!x.IsDeleted || provideDeleted));

            return J<ISchemaEntity>.AsTask(value);
        }

        public Task<J<ISchemaEntity>> GetSchemaAsync(string name, bool provideDeleted = false)
        {
            var value = State.Value.FindSchema(x => string.Equals(x.Name, name, StringComparison.OrdinalIgnoreCase) && (!x.IsDeleted || provideDeleted));

            return J<ISchemaEntity>.AsTask(value);
        }

        public Task HandleAsync(J<Envelope<IEvent>> message)
        {
            State = State.Update(v => v.Apply(message, fieldRegistry));

            return WriteStateAsync();
        }
    }
}
