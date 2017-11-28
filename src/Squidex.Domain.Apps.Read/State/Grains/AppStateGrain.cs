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
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Domain.Apps.Events;
using Squidex.Domain.Apps.Events.Apps;
using Squidex.Domain.Apps.Read.Apps;
using Squidex.Domain.Apps.Read.Rules;
using Squidex.Domain.Apps.Read.Schemas;
using Squidex.Infrastructure;
using Squidex.Infrastructure.CQRS.Events;
using Squidex.Infrastructure.States;

namespace Squidex.Domain.Apps.Read.State.Grains
{
    public class AppStateGrain : StatefulObject<AppStateGrainState>
    {
        private readonly FieldRegistry fieldRegistry;
        private Exception exception;

        public AppStateGrain(FieldRegistry fieldRegistry)
        {
            Guard.NotNull(fieldRegistry, nameof(fieldRegistry));

            this.fieldRegistry = fieldRegistry;
        }

        public override async Task ReadStateAsync()
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

            State.SetRegistry(fieldRegistry);
        }

        public virtual Task<(IAppEntity, ISchemaEntity)> GetAppWithSchemaAsync(Guid id)
        {
            var schema = State.FindSchema(x => x.Id == id && !x.IsDeleted);

            return Task.FromResult((State.GetApp(), schema));
        }

        public virtual Task<IAppEntity> GetAppAsync()
        {
            var result = State.GetApp();

            return Task.FromResult(result);
        }

        public virtual Task<List<IRuleEntity>> GetRulesAsync()
        {
            var result = State.FindRules();

            return Task.FromResult(result);
        }

        public virtual Task<List<ISchemaEntity>> GetSchemasAsync()
        {
            var result = State.FindSchemas(x => !x.IsDeleted);

            return Task.FromResult(result);
        }

        public virtual Task<ISchemaEntity> GetSchemaAsync(Guid id, bool provideDeleted = false)
        {
            var result = State.FindSchema(x => x.Id == id && (!x.IsDeleted || provideDeleted));

            return Task.FromResult(result);
        }

        public virtual Task<ISchemaEntity> GetSchemaAsync(string name, bool provideDeleted = false)
        {
            var result = State.FindSchema(x => string.Equals(x.Name, name, StringComparison.OrdinalIgnoreCase) && (!x.IsDeleted || provideDeleted));

            return Task.FromResult(result);
        }

        public async virtual Task HandleAsync(Envelope<IEvent> message)
        {
            if (exception != null)
            {
                if (message.Payload is AppCreated)
                {
                    exception = null;
                }
                else
                {
                    throw exception;
                }
            }

            if (message.Payload is AppEvent appEvent && (State.App == null || State.App.Id == appEvent.AppId.Id))
            {
                try
                {
                    State = State.Apply(message);

                    await WriteStateAsync();
                }
                catch (InconsistentStateException)
                {
                    await ReadStateAsync();

                    State = State.Apply(message);

                    await WriteStateAsync();
                }
            }
        }
    }
}
