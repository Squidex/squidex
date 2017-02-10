// ==========================================================================
//  IEventCatchConsumer.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System.Threading.Tasks;

namespace Squidex.Infrastructure.CQRS.Events
{
    public interface IEventCatchConsumer
    {
        Task<long> GetLastHandledEventNumber();

        Task On(Envelope<IEvent> @event, long eventNumber);
    }
}