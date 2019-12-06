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
using Squidex.Infrastructure.States;

namespace Squidex.Domain.Apps.Entities.Backup
{
    public abstract class BackupHandlerWithStore : IBackupHandler
    {
        private readonly IStore<Guid> store;

        public abstract string Name { get; }

        protected BackupHandlerWithStore(IStore<Guid> store)
        {
            Guard.NotNull(store);

            this.store = store;
        }

        protected async Task RebuildManyAsync(IEnumerable<Guid> ids, Func<Guid, Task> action)
        {
            foreach (var id in ids)
            {
                await action(id);
            }
        }

        protected async Task RebuildAsync<TState, TGrain>(Guid key) where TState : IDomainState<TState>, new()
        {
            var state = new TState
            {
                Version = EtagVersion.Empty
            };

            var persistence = store.WithSnapshotsAndEventSourcing(typeof(TGrain), key, (TState s) => state = s, e =>
            {
                state = state.Apply(e);

                state.Version++;
            });

            await persistence.ReadAsync();
            await persistence.WriteSnapshotAsync(state);
        }
    }
}
