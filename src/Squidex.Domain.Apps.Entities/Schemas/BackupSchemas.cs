// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Orleans;
using Squidex.Domain.Apps.Entities.Backup;
using Squidex.Domain.Apps.Entities.Schemas.Indexes;
using Squidex.Domain.Apps.Events.Schemas;
using Squidex.Infrastructure;
using Squidex.Infrastructure.EventSourcing;
using Squidex.Infrastructure.Tasks;

namespace Squidex.Domain.Apps.Entities.Schemas
{
    public sealed class BackupSchemas : BackupHandler
    {
        private readonly Dictionary<string, Guid> schemasByName = new Dictionary<string, Guid>();
        private readonly IGrainFactory grainFactory;

        public override string Name { get; } = "Schemas";

        public BackupSchemas(IGrainFactory grainFactory)
        {
            Guard.NotNull(grainFactory, nameof(grainFactory));

            this.grainFactory = grainFactory;
        }

        public override Task<bool> RestoreEventAsync(Envelope<IEvent> @event, Guid appId, BackupReader reader, RefToken actor)
        {
            switch (@event.Payload)
            {
                case SchemaCreated schemaCreated:
                    schemasByName[schemaCreated.SchemaId.Name] = schemaCreated.SchemaId.Id;
                    break;
                case SchemaDeleted schemaDeleted:
                    schemasByName.Remove(schemaDeleted.SchemaId.Name);
                    break;
            }

            return TaskHelper.True;
        }

        public override async Task RestoreAsync(Guid appId, BackupReader reader)
        {
            await grainFactory.GetGrain<ISchemasByAppIndex>(appId).RebuildAsync(schemasByName);
        }
    }
}
