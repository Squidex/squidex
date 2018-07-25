// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Squidex.Domain.Apps.Entities.Contents;
using Squidex.Domain.Apps.Entities.Contents.State;
using Squidex.Domain.Apps.Events.Contents;
using Squidex.Infrastructure.EventSourcing;
using Squidex.Infrastructure.States;
using Squidex.Infrastructure.Tasks;

namespace Squidex.Domain.Apps.Entities.Backup.Handlers
{
    public sealed class RestoreContents : HandlerBase, IRestoreHandler
    {
        private readonly HashSet<Guid> contentIds = new HashSet<Guid>();

        public string Name { get; } = "Contents";

        public RestoreContents(IStore<Guid> store)
            : base(store)
        {
        }

        public Task HandleAsync(Envelope<IEvent> @event, Stream attachment)
        {
            switch (@event.Payload)
            {
                case ContentCreated contentCreated:
                    contentIds.Add(contentCreated.ContentId);
                    break;
            }

            return TaskHelper.Done;
        }

        public Task ProcessAsync()
        {
            return RebuildManyAsync(contentIds, id => RebuildAsync<ContentState, ContentGrain>(id, (e, s) => s.Apply(e)));
        }

        public Task CompleteAsync()
        {
            return TaskHelper.Done;
        }
    }
}
