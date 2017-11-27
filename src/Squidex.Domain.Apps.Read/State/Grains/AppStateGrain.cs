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
using Squidex.Infrastructure.Tasks;

namespace Squidex.Domain.Apps.Read.State.Grains
{
    public class AppStateGrain : StatefulObject<AppStateGrainState>
    {
        private readonly FieldRegistry fieldRegistry;
        private readonly TaskFactory taskFactory = new TaskFactory(new LimitedConcurrencyLevelTaskScheduler(1));
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
            return taskFactory.StartNew(() =>
            {
                var schema = State.FindSchema(x => x.Id == id && !x.IsDeleted);

                return (State.GetApp(), schema);
            });
        }

        public virtual Task<IAppEntity> GetAppAsync()
        {
            return taskFactory.StartNew(() =>
            {
                var value = State.GetApp();

                return value;
            });
        }

        public virtual Task<List<IRuleEntity>> GetRulesAsync()
        {
            return taskFactory.StartNew(() =>
            {
                var value = State.FindRules();

                return value;
            });
        }

        public virtual Task<List<ISchemaEntity>> GetSchemasAsync()
        {
            return taskFactory.StartNew(() =>
            {
                var value = State.FindSchemas(x => !x.IsDeleted);

                return value;
            });
        }

        public virtual Task<ISchemaEntity> GetSchemaAsync(Guid id, bool provideDeleted = false)
        {
            return taskFactory.StartNew(() =>
            {
                var value = State.FindSchema(x => x.Id == id && (!x.IsDeleted || provideDeleted));

                return value;
            });
        }

        public virtual Task<ISchemaEntity> GetSchemaAsync(string name, bool provideDeleted = false)
        {
            return taskFactory.StartNew(() =>
            {
                var value = State.FindSchema(x => string.Equals(x.Name, name, StringComparison.OrdinalIgnoreCase) && (!x.IsDeleted || provideDeleted));

                return value;
            });
        }

        public virtual Task HandleAsync(Envelope<IEvent> message)
        {
            return taskFactory.StartNew(() =>
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

                if (message.Payload is AppEvent appEvent)
                {
                    if (State.App == null || State.App.Id == appEvent.AppId.Id)
                    {
                        State.Apply(message);
                    }
                }

                return WriteStateAsync();
            }).Unwrap();
        }
    }
}
