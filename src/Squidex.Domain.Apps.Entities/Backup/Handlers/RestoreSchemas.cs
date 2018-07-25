// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Orleans;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Domain.Apps.Entities.Schemas;
using Squidex.Domain.Apps.Entities.Schemas.State;
using Squidex.Domain.Apps.Events.Apps;
using Squidex.Domain.Apps.Events.Schemas;
using Squidex.Infrastructure;
using Squidex.Infrastructure.EventSourcing;
using Squidex.Infrastructure.States;
using Squidex.Infrastructure.Tasks;

namespace Squidex.Domain.Apps.Entities.Backup.Handlers
{
    public sealed class RestoreSchemas : HandlerBase, IRestoreHandler
    {
        private readonly HashSet<NamedId<Guid>> schemaIds = new HashSet<NamedId<Guid>>();
        private readonly Dictionary<string, Guid> schemasByName = new Dictionary<string, Guid>();
        private readonly FieldRegistry fieldRegistry;
        private readonly IGrainFactory grainFactory;
        private Guid appId;

        public string Name { get; } = "Schemas";

        public RestoreSchemas(IStore<Guid> store, FieldRegistry fieldRegistry, IGrainFactory grainFactory)
            : base(store)
        {
            Guard.NotNull(fieldRegistry, nameof(fieldRegistry));
            Guard.NotNull(grainFactory, nameof(grainFactory));

            this.fieldRegistry = fieldRegistry;

            this.grainFactory = grainFactory;
        }

        public Task HandleAsync(Envelope<IEvent> @event, Stream attachment)
        {
            switch (@event.Payload)
            {
                case AppCreated appCreated:
                    appId = appCreated.AppId.Id;
                    break;
                case SchemaCreated schemaCreated:
                    schemaIds.Add(schemaCreated.SchemaId);
                    schemasByName[schemaCreated.SchemaId.Name] = schemaCreated.SchemaId.Id;
                    break;
            }

            return TaskHelper.Done;
        }

        public async Task ProcessAsync()
        {
            await RebuildManyAsync(schemaIds.Select(x => x.Id), id => RebuildAsync<SchemaState, SchemaGrain>(id, (e, s) => s.Apply(e, fieldRegistry)));

            await grainFactory.GetGrain<ISchemasByAppIndex>(appId).RebuildAsync(schemasByName);
        }

        public Task CompleteAsync()
        {
            return TaskHelper.Done;
        }
    }
}
