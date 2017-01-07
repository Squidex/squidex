// ==========================================================================
//  IHistoryEventCreator.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System.Collections.Generic;
using System.Threading.Tasks;
using Squidex.Infrastructure.CQRS;
using Squidex.Infrastructure.CQRS.Events;

namespace Squidex.Read.History
{
    public interface IHistoryEventsCreator
    {
        IReadOnlyDictionary<string, string> Texts { get; }

        Task<HistoryEventToStore> CreateEventAsync(Envelope<IEvent> @event);
    }
}
