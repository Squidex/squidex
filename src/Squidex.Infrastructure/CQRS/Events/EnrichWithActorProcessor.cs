// ==========================================================================
//  EnrichWithActorProcessor.cs
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
    public sealed class EnrichWithActorProcessor : IEventProcessor
    {
        public Task ProcessEventAsync(Envelope<IEvent> @event, IAggregate aggregate, ICommand command)
        {
            var userCommand = command as IActorCommand;

            if (userCommand != null)
            {
                @event.SetActor(userCommand.Actor);
            }

            return TaskHelper.Done;
        }
    }
}
