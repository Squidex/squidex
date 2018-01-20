// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Threading.Tasks;

namespace Squidex.Infrastructure.EventSourcing
{
    public delegate IEventConsumer EventConsumerFactory(string name);

    public interface IEventConsumer
    {
        string Name { get; }

        string EventsFilter { get; }

        Task ClearAsync();

        Task On(Envelope<IEvent> @event);
    }
}