﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Squidex.Domain.Apps.Entities.Backup;
using Squidex.Domain.Apps.Entities.Schemas.Indexes;
using Squidex.Domain.Apps.Events.Schemas;
using Squidex.Infrastructure;
using Squidex.Infrastructure.EventSourcing;
using Squidex.Infrastructure.Tasks;

namespace Squidex.Domain.Apps.Entities.Schemas
{
    public sealed class BackupSchemas : IBackupHandler
    {
        private readonly Dictionary<string, Guid> schemasByName = new Dictionary<string, Guid>();
        private readonly ISchemasIndex indexSchemas;

        public string Name { get; } = "Schemas";

        public BackupSchemas(ISchemasIndex indexSchemas)
        {
            Guard.NotNull(indexSchemas);

            this.indexSchemas = indexSchemas;
        }

        public Task<bool> RestoreEventAsync(Envelope<IEvent> @event, RestoreContext context)
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

        public Task RestoreAsync(RestoreContext context)
        {
            return indexSchemas.RebuildAsync(context.AppId, schemasByName);
        }
    }
}
