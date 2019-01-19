// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Squidex.Domain.Apps.Entities.Backup;
using Squidex.Domain.Apps.Entities.Contents.State;
using Squidex.Domain.Apps.Events.Contents;
using Squidex.Domain.Apps.Events.Schemas;
using Squidex.Infrastructure;
using Squidex.Infrastructure.EventSourcing;
using Squidex.Infrastructure.States;
using Squidex.Infrastructure.Tasks;

namespace Squidex.Domain.Apps.Entities.Contents
{
    public sealed class BackupContents : BackupHandlerWithStore
    {
        private readonly Dictionary<Guid, HashSet<Guid>> contentIdsBySchemaId = new Dictionary<Guid, HashSet<Guid>>();

        public override string Name { get; } = "Contents";

        public BackupContents(IStore<Guid> store)
            : base(store)
        {
        }

        public override Task<bool> RestoreEventAsync(Envelope<IEvent> @event, Guid appId, BackupReader reader, RefToken actor)
        {
            switch (@event.Payload)
            {
                case ContentCreated contentCreated:
                    contentIdsBySchemaId.GetOrAddNew(contentCreated.SchemaId.Id).Add(contentCreated.ContentId);
                    break;
                case SchemaDeleted schemaDeleted:
                    contentIdsBySchemaId.Remove(schemaDeleted.SchemaId.Id);
                    break;
            }

            return TaskHelper.True;
        }

        public override Task RestoreAsync(Guid appId, BackupReader reader)
        {
            var contentIds = contentIdsBySchemaId.Values.SelectMany(x => x);

            return RebuildManyAsync(contentIds, id => RebuildAsync<ContentState, ContentGrain>(id, (e, s) => s.Apply(e)));
        }
    }
}
