// ==========================================================================
//  UpdateStateBlock.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System.Threading.Tasks;
using Squidex.Infrastructure.Log;

namespace Squidex.Infrastructure.CQRS.Events.Internal
{
    public sealed class UpdateStateBlock : EventReceiverBlock<Envelope<IEvent>, Envelope<IEvent>>
    {
        public UpdateStateBlock(IEventConsumer eventConsumer, IEventConsumerInfoRepository eventConsumerInfoRepository, ISemanticLog log) 
            : base(false, eventConsumer, eventConsumerInfoRepository, log)
        {
        }

        protected override async Task<Envelope<IEvent>> On(Envelope<IEvent> input)
        {
            await EventConsumerInfoRepository.SetLastHandledEventNumberAsync(EventConsumer.Name, input.Headers.EventNumber());

            return input;
        }

        protected override long GetEventNumber(Envelope<IEvent> input)
        {
            return input.Headers.EventNumber();
        }
    }
}
