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
using Orleans.Runtime;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Domain.Apps.Events.Apps;
using Squidex.Domain.Apps.Read.Apps;
using Squidex.Domain.Apps.Read.Rules;
using Squidex.Domain.Apps.Read.Schemas;
using Squidex.Infrastructure;
using Squidex.Infrastructure.CQRS.Events;
using Squidex.Infrastructure.Json.Orleans;
using Squidex.Infrastructure.Orleans;

namespace Squidex.Domain.Apps.Read.State.Orleans.Grains.Implementations
{
    public sealed class AppStateGrain : GrainV2<AppStateGrainState>, IAppStateGrain
    {
        private readonly FieldRegistry fieldRegistry;
        private Exception exception;

        public AppStateGrain(FieldRegistry fieldRegistry, IGrainRuntime runtime)
            : base(runtime)
        {
            Guard.NotNull(fieldRegistry, nameof(fieldRegistry));
            Guard.NotNull(runtime, nameof(runtime));

            this.fieldRegistry = fieldRegistry;
        }

        protected override async Task ReadStateAsync()
        {
            try
            {
                await base.ReadStateAsync();
            }
            catch (Exception ex)
            {
                exception = ex;

                State = new AppStateGrainState();
            }
        }

        public override Task OnActivateAsync()
        {
            State.SetRegistry(fieldRegistry);

            return base.OnActivateAsync();
        }

        public Task<J<(IAppEntity, ISchemaEntity)>> GetAppWithSchemaAsync(Guid id)
        {
            var schema = State.FindSchema(x => x.Id == id && !x.IsDeleted);

            return Task.FromResult((State.GetApp(), schema).AsJ());
        }

        public Task<J<IAppEntity>> GetAppAsync()
        {
            var value = State.GetApp();

            return Task.FromResult(value.AsJ());
        }

        public Task<J<List<IRuleEntity>>> GetRulesAsync()
        {
            var value = State.FindRules();

            return Task.FromResult(value.AsJ());
        }

        public Task<J<List<ISchemaEntity>>> GetSchemasAsync()
        {
            var value = State.FindSchemas(x => !x.IsDeleted);

            return Task.FromResult(value.AsJ());
        }

        public Task<J<ISchemaEntity>> GetSchemaAsync(Guid id, bool provideDeleted = false)
        {
            var value = State.FindSchema(x => x.Id == id && (!x.IsDeleted || provideDeleted));

            return Task.FromResult(value.AsJ());
        }

        public Task<J<ISchemaEntity>> GetSchemaAsync(string name, bool provideDeleted = false)
        {
            var value = State.FindSchema(x => string.Equals(x.Name, name, StringComparison.OrdinalIgnoreCase) && (!x.IsDeleted || provideDeleted));

            return Task.FromResult(value.AsJ());
        }

        public Task HandleAsync(J<Envelope<IEvent>> message)
        {
            if (exception != null)
            {
                if (message.Value.Payload is AppCreated)
                {
                    exception = null;
                }
                else
                {
                    throw exception;
                }
            }

            State.Apply(message.Value);

            return WriteStateAsync();
        }
    }
}
