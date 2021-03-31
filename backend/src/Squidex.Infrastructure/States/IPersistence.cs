// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Threading.Tasks;
using Squidex.Infrastructure.EventSourcing;

namespace Squidex.Infrastructure.States
{
    public interface IPersistence<in TState>
    {
        long Version { get; }

        bool IsSnapshotStale { get; }

        Task DeleteAsync();

        Task WriteEventsAsync(IReadOnlyList<Envelope<IEvent>> events);

        Task WriteSnapshotAsync(TState state);

        Task ReadAsync(long expectedVersion = EtagVersion.Any);
    }
}
