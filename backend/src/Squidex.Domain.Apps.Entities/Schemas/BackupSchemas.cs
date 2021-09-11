// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Threading.Tasks;
using Squidex.Domain.Apps.Entities.Backup;
using Squidex.Domain.Apps.Entities.Schemas.DomainObject;
using Squidex.Domain.Apps.Events.Schemas;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Commands;
using Squidex.Infrastructure.EventSourcing;

namespace Squidex.Domain.Apps.Entities.Schemas
{
    public sealed class BackupSchemas : IBackupHandler
    {
        private const int BatchSize = 100;
        private readonly HashSet<DomainId> schemaIds = new HashSet<DomainId>();
        private readonly Rebuilder rebuilder;

        public string Name { get; } = "Schemas";

        public BackupSchemas(Rebuilder rebuilder)
        {
            this.rebuilder = rebuilder;
        }

        public Task<bool> RestoreEventAsync(Envelope<IEvent> @event, RestoreContext context)
        {
            switch (@event.Payload)
            {
                case SchemaCreated schemaCreated:
                    schemaIds.Add(schemaCreated.SchemaId.Id);
                    break;
                case SchemaDeleted schemaDeleted:
                    schemaIds.Remove(schemaDeleted.SchemaId.Id);
                    break;
            }

            return Task.FromResult(true);
        }

        public async Task RestoreAsync(RestoreContext context)
        {
            if (schemaIds.Count > 0)
            {
                await rebuilder.InsertManyAsync<SchemaDomainObject, SchemaDomainObject.State>(schemaIds, BatchSize);
            }
        }
    }
}
