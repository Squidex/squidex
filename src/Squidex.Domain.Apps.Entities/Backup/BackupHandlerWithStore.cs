// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Commands;
using Squidex.Infrastructure.EventSourcing;
using Squidex.Infrastructure.States;

namespace Squidex.Domain.Apps.Entities.Backup
{
    public abstract class BackupHandlerWithStore : BackupHandler
    {
        private readonly IStore<Guid> store;

        protected BackupHandlerWithStore(IStore<Guid> store)
        {
            Guard.NotNull(store, nameof(store));

            this.store = store;
        }

        protected Task RemoveSnapshotAsync<TState>(Guid id)
        {
            return store.RemoveSnapshotAsync<Guid, TState>(id);
        }

        protected async Task RebuildManyAsync(IEnumerable<Guid> ids, Func<Guid, Task> action)
        {
            foreach (var id in ids)
            {
                await action(id);
            }
        }

        protected async Task RebuildAsync<TState, TGrain>(Guid key, Func<Envelope<IEvent>, TState, TState> func) where TState : IDomainState, new()
        {
            var state = new TState
            {
                Version = EtagVersion.Empty
            };

            var persistence = store.WithSnapshotsAndEventSourcing<TState, Guid>(typeof(TGrain), key, s => state = s, e =>
            {
                state = func(e, state);

                state.Version++;
            });

            await persistence.ReadAsync();
            await persistence.WriteSnapshotAsync(state);
        }
    }
}
