// ==========================================================================
//  EnrichWithAggregateIdProcessor.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System.Threading.Tasks;
using Squidex.Infrastructure.CQRS.Commands;
using Squidex.Infrastructure.Tasks;

namespace Squidex.Infrastructure.CQRS.Events
{
    public sealed class EnrichWithAggregateIdProcessor : IEventProcessor
    {
        public Task ProcessEventAsync(Envelope<IEvent> @event, IAggregate aggregate, ICommand command)
        {
            var aggregateCommand = command as IAggregateCommand;

            if (aggregateCommand != null)
            {
                @event.SetAggregateId(aggregateCommand.AggregateId);
            }

            return TaskHelper.Done;
        }
    }
}
