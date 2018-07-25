// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.IO;
using System.Threading.Tasks;
using Squidex.Domain.Apps.Events.Apps;
using Squidex.Infrastructure;
using Squidex.Infrastructure.EventSourcing;
using Squidex.Infrastructure.States;
using Squidex.Infrastructure.Tasks;

namespace Squidex.Domain.Apps.Entities.Backup.Handlers
{
    public sealed class RestoreApp : HandlerBase, IRestoreHandler
    {
        private NamedId<Guid> appId;

        public string Name { get; } = "App";

        public RestoreApp(IStore<Guid> store)
            : base(store)
        {
        }

        public Task HandleAsync(Envelope<IEvent> @event, Stream attachment)
        {
            if (@event.Payload is AppCreated appCreated)
            {
                appId = appCreated.AppId;
            }

            return TaskHelper.Done;
        }

        public Task ProcessAsync()
        {
            return TaskHelper.Done;
        }

        public Task CompleteAsync()
        {
            return TaskHelper.Done;
        }
    }
}
