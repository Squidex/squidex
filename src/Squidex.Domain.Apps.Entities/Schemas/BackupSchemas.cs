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
using Squidex.Domain.Apps.Core.Schemas;
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
        private readonly HashSet<NamedId<Guid>> schemaIds = new HashSet<NamedId<Guid>>();
        private readonly Dictionary<string, Guid> schemasByName = new Dictionary<string, Guid>();
        private readonly FieldRegistry fieldRegistry;
        private readonly IGrainFactory grainFactory;

        public override string Name { get; } = "Schemas";

        public BackupSchemas(FieldRegistry fieldRegistry, IGrainFactory grainFactory)
        {
            Guard.NotNull(fieldRegistry, nameof(fieldRegistry));
            Guard.NotNull(grainFactory, nameof(grainFactory));

            this.fieldRegistry = fieldRegistry;

            this.grainFactory = grainFactory;
        }

        public override Task<bool> RestoreEventAsync(Envelope<IEvent> @event, Guid appId, BackupReader reader, RefToken actor)
        {
            switch (@event.Payload)
            {
                case SchemaCreated schemaCreated:
                    schemaIds.Add(schemaCreated.SchemaId);
                    schemasByName[schemaCreated.SchemaId.Name] = schemaCreated.SchemaId.Id;
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
