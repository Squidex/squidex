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
using Squidex.Infrastructure.Commands;
using Squidex.Infrastructure.EventSourcing;
using Squidex.Infrastructure.Tasks;

namespace Squidex.Domain.Apps.Entities.Contents
{
    public sealed class BackupContents : IBackupHandler
    {
        private readonly Dictionary<Guid, HashSet<Guid>> contentIdsBySchemaId = new Dictionary<Guid, HashSet<Guid>>();
        private readonly Rebuilder rebuilder;

        public string Name { get; } = "Contents";

        public BackupContents(Rebuilder rebuilder)
        {
            Guard.NotNull(rebuilder);

            this.rebuilder = rebuilder;
        }

        public Task<bool> RestoreEventAsync(Envelope<IEvent> @event, RestoreContext context)
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

        public async Task RestoreAsync(RestoreContext context)
        {
            if (contentIdsBySchemaId.Count > 0)
            {
                await rebuilder.InsertManyAsync<ContentDomainObject, ContentState>(async target =>
                {
                    foreach (var contentId in contentIdsBySchemaId.Values.SelectMany(x => x))
                    {
                        await target(contentId);
                    }
                });
            }
        }
    }
}
