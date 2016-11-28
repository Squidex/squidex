// ==========================================================================
//  IEventProcessor.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System.Threading.Tasks;
using Squidex.Infrastructure.CQRS.Commands;

namespace Squidex.Infrastructure.CQRS.Events
{
    public interface IEventProcessor
    {
        Task ProcessEventAsync(Envelope<IEvent> @event, IAggregate aggregate, ICommand command);
    }
}
