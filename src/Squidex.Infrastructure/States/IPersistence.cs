// ==========================================================================
//  IPersistent.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System.Threading.Tasks;
using Squidex.Infrastructure.CQRS.Events;

namespace Squidex.Infrastructure.States
{
    public interface IPersistence<TState>
    {
        Task WriteEventsAsync(params Envelope<IEvent>[] @events);

        Task WriteSnapShotAsync(TState state);

        Task ReadAsync(bool force = false);
    }
}
