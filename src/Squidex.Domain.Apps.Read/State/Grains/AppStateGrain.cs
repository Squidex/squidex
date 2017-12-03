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
    public class AppStateGrain : IStatefulObject
    {
        private readonly FieldRegistry fieldRegistry;
        private IPersistence<AppStateGrainState> persistence;
        private Task readTask;
        private Exception exception;
        private AppStateGrainState state;

        public AppStateGrain(FieldRegistry fieldRegistry)
        {
            Guard.NotNull(fieldRegistry, nameof(fieldRegistry));

            this.fieldRegistry = fieldRegistry;
        }

        public Task ActivateAsync(string key, IStore store)
        {
            persistence = store.WithSnapshots<AppStateGrain, AppStateGrainState>(key, s => state = s);

            return readTask ?? (readTask = ReadInitialAsync());
        }

        private async Task ReadInitialAsync()
        {
            try
            {
                await persistence.ReadAsync();
            }
            catch (Exception ex)
            {
                exception = ex;

                state = new AppStateGrainState();
            }

            state.SetRegistry(fieldRegistry);
        }

        public virtual Task<(IAppEntity, ISchemaEntity)> GetAppWithSchemaAsync(Guid id)
        {
            var schema = state.FindSchema(x => x.Id == id && !x.IsDeleted);

            return Task.FromResult((state.GetApp(), schema));
        }

        public virtual Task<IAppEntity> GetAppAsync()
        {
            var result = state.GetApp();

            return Task.FromResult(result);
        }

        public virtual Task<List<IRuleEntity>> GetRulesAsync()
        {
            var result = state.FindRules();

            return Task.FromResult(result);
        }

        public virtual Task<List<ISchemaEntity>> GetSchemasAsync()
        {
            var result = state.FindSchemas(x => !x.IsDeleted);

            return Task.FromResult(result);
        }

        public virtual Task<ISchemaEntity> GetSchemaAsync(Guid id, bool provideDeleted = false)
        {
            var result = state.FindSchema(x => x.Id == id && (!x.IsDeleted || provideDeleted));

            return Task.FromResult(result);
        }

        public virtual Task<ISchemaEntity> GetSchemaAsync(string name, bool provideDeleted = false)
        {
            var result = state.FindSchema(x => string.Equals(x.Name, name, StringComparison.OrdinalIgnoreCase) && (!x.IsDeleted || provideDeleted));

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

            if (message.Payload is AppEvent appEvent && (state.App == null || state.App.Id == appEvent.AppId.Id))
            {
                try
                {
                    state = state.Apply(message);

                    await persistence.WriteSnapShotAsync(state);
                }
                catch (InconsistentStateException)
                {
                    await persistence.ReadAsync(true);

                    state = state.Apply(message);

                    await persistence.WriteSnapShotAsync(state);
                }
            }
        }
    }
}
