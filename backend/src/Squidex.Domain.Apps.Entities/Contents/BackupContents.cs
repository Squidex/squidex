// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Squidex.Domain.Apps.Entities.Backup;
using Squidex.Domain.Apps.Entities.Contents.State;
using Squidex.Domain.Apps.Events.Contents;
using Squidex.Domain.Apps.Events.Schemas;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Commands;
using Squidex.Infrastructure.EventSourcing;

namespace Squidex.Domain.Apps.Entities.Contents
{
    public sealed class BackupContents : IBackupHandler
    {
        private readonly Dictionary<DomainId, HashSet<DomainId>> contentIdsBySchemaId = new Dictionary<DomainId, HashSet<DomainId>>();
        private readonly Rebuilder rebuilder;

        public string Name { get; } = "Contents";

        public BackupContents(Rebuilder rebuilder)
        {
            Guard.NotNull(rebuilder, nameof(rebuilder));

            this.rebuilder = rebuilder;
        }

        public Task<bool> RestoreEventAsync(Envelope<IEvent> @event, RestoreContext context)
        {
            switch (@event.Payload)
            {
                case ContentCreated contentCreated:
                    contentIdsBySchemaId.GetOrAddNew(contentCreated.SchemaId.Id).Add(@event.Headers.AggregateId());
                    break;
                case SchemaDeleted schemaDeleted:
                    contentIdsBySchemaId.Remove(schemaDeleted.SchemaId.Id);
                    break;
            }

            return Task.FromResult(true);
        }

        public async Task RestoreAsync(RestoreContext context)
        {
            var ids = contentIdsBySchemaId.Values.SelectMany(x => x);

            if (ids.Any())
            {
                await rebuilder.InsertManyAsync<ContentDomainObject, ContentState>(ids);
            }
        }
    }
}
