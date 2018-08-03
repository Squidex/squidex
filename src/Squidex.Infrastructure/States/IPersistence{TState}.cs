﻿// ==========================================================================
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
    public interface IPersistence<TState>
    {
        long Version { get; }

        Task DeleteAsync();

        Task WriteEventsAsync(IEnumerable<Envelope<IEvent>> @events);

        Task WriteSnapshotAsync(TState state);

        Task ReadAsync(long expectedVersion = EtagVersion.Any);
    }
}
