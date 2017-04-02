// ==========================================================================
//  ParseEventBlock.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Threading.Tasks;
using Squidex.Infrastructure.Log;

namespace Squidex.Infrastructure.CQRS.Events.Internal
{
    internal sealed class ParseEventBlock : EventReceiverBlock<StoredEvent, Envelope<IEvent>>
    {
        private readonly EventDataFormatter formatter;

        public ParseEventBlock(IEventConsumer eventConsumer, IEventConsumerInfoRepository eventConsumerInfoRepository, ISemanticLog log, EventDataFormatter formatter) 
            : base(true, eventConsumer, eventConsumerInfoRepository, log)
        {
            this.formatter = formatter;
        }

        protected override Task<Envelope<IEvent>> On(StoredEvent input)
        {
            Envelope<IEvent> result = null;
            try
            {
                result = formatter.Parse(input.Data);

                result.SetEventNumber(input.EventNumber);
                result.SetEventStreamNumber(input.EventStreamNumber);
            }
            catch (Exception ex)
            {
                Log.LogFatal(ex, w => w
                    .WriteProperty("action", "ParseEvent")
                    .WriteProperty("state", "Failed")
                    .WriteProperty("eventId", input.Data.EventId.ToString())
                    .WriteProperty("eventNumber", input.EventNumber));
            }

            return Task.FromResult(result);
        }

        protected override long GetEventNumber(StoredEvent input)
        {
            return input.EventNumber;
        }
    }
}
