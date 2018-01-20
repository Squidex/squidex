// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Threading.Tasks;
using Squidex.Infrastructure.EventSourcing;

namespace Squidex.Infrastructure.States
{
    public interface IPersistence : IPersistence<object>
    {
    }

    public interface IPersistence<TState>
    {
        long Version { get; }

        Task WriteEventsAsync(IEnumerable<Envelope<IEvent>> @events);

        Task WriteSnapshotAsync(TState state);

        Task ReadAsync(long expectedVersion = EtagVersion.Any);
    }
}
