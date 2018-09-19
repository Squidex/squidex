// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Squidex.Domain.Apps.Entities.Backup;
using Squidex.Domain.Apps.Entities.Contents.State;
using Squidex.Domain.Apps.Events.Contents;
using Squidex.Infrastructure;
using Squidex.Infrastructure.EventSourcing;
using Squidex.Infrastructure.States;
using Squidex.Infrastructure.Tasks;

namespace Squidex.Domain.Apps.Entities.Contents
{
    public sealed class BackupContents : BackupHandlerWithStore
    {
        private readonly HashSet<Guid> contentIds = new HashSet<Guid>();

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
                    contentIds.Add(contentCreated.ContentId);
                    break;
            }

            return TaskHelper.True;
        }

        public override Task RestoreAsync(Guid appId, BackupReader reader)
        {
            return RebuildManyAsync(contentIds, id => RebuildAsync<ContentState, ContentGrain>(id, (e, s) => s.Apply(e)));
        }
    }
}
