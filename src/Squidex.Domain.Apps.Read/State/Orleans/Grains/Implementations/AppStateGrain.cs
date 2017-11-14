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
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Domain.Apps.Read.Apps;
using Squidex.Domain.Apps.Read.Rules;
using Squidex.Domain.Apps.Read.Schemas;
using Squidex.Infrastructure;
using Squidex.Infrastructure.CQRS.Events;
using Squidex.Infrastructure.Json.Orleans;

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

        public Task<J<(IAppEntity, ISchemaEntity)>> GetAppWithSchemaAsync(Guid id)
        {
            var schema = State.FindSchema(x => x.Id == id && !x.IsDeleted);

            return J<(IAppEntity AppEntity, ISchemaEntity SchemaEntity)>.AsTask((State.GetApp(), schema));
        }

        public Task<J<IAppEntity>> GetAppAsync()
        {
            var value = State.GetApp();

            return J<IAppEntity>.AsTask(value);
        }

        public Task<J<List<IRuleEntity>>> GetRulesAsync()
        {
            var value = State.FindRules();

            return J<List<IRuleEntity>>.AsTask(value);
        }

        public Task<J<List<ISchemaEntity>>> GetSchemasAsync()
        {
            var value = State.FindSchemas(x => !x.IsDeleted);

            return J<List<ISchemaEntity>>.AsTask(value);
        }

        public Task<J<ISchemaEntity>> GetSchemaAsync(Guid id, bool provideDeleted = false)
        {
            var value = State.FindSchema(x => x.Id == id && (!x.IsDeleted || provideDeleted));

            return J<ISchemaEntity>.AsTask(value);
        }

        public Task<J<ISchemaEntity>> GetSchemaAsync(string name, bool provideDeleted = false)
        {
            var value = State.FindSchema(x => string.Equals(x.Name, name, StringComparison.OrdinalIgnoreCase) && (!x.IsDeleted || provideDeleted));

            return J<ISchemaEntity>.AsTask(value);
        }

        public Task HandleAsync(J<Envelope<IEvent>> message)
        {
            State.Apply(message, fieldRegistry);

            return WriteStateAsync();
        }
    }
}
