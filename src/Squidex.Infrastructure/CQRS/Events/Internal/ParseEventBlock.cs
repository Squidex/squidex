// ==========================================================================
//  ParseEventBlock.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Squidex.Infrastructure.Log;

namespace Squidex.Infrastructure.CQRS.Events.Internal
{
    internal sealed class ParseEventBlock : IEventReceiverBlock
    {
        private readonly EventDataFormatter formatter;
        private readonly ISemanticLog log;
        private readonly TransformBlock<StoredEvent, Envelope<IEvent>> transformBlock;
        private long lastReceivedEventNumber = -1;
        private bool isRunning = true;

        public Action<Exception> OnError { get; set; }

        public ParseEventBlock(EventDataFormatter formatter, ISemanticLog log)
        {
            this.formatter = formatter;
            this.log = log;

            var nullHandler = new ActionBlock<Envelope<IEvent>>(x => { });

            transformBlock =
                new TransformBlock<StoredEvent, Envelope<IEvent>>(x => HandleAsync(x),
                    new ExecutionDataflowBlockOptions { BoundedCapacity = 1 });
            transformBlock.LinkTo(nullHandler, x => x == null);
        }

        public Task NextAsync(StoredEvent input)
        {
            return transformBlock.SendAsync(input);
        }

        public void LinkTo(ITargetBlock<Envelope<IEvent>> target)
        {
            transformBlock.LinkTo(target, new DataflowLinkOptions { PropagateCompletion = true });
        }

        public void Complete()
        {
            transformBlock.Complete();
        }

        public void Stop()
        {
            isRunning = false;
        }

        public void Reset()
        {
            isRunning = true;

            lastReceivedEventNumber = -1;
        }

        private Envelope<IEvent> HandleAsync(StoredEvent input)
        {
            var eventNumber = input.EventNumber;

            if (eventNumber <= lastReceivedEventNumber || !isRunning)
            {
                return null;
            }

            try
            {
                var result = formatter.Parse(input.Data);

                result.SetEventNumber(input.EventNumber);
                result.SetEventStreamNumber(input.EventStreamNumber);

                return result;
            }
            catch (Exception ex)
            {
                OnError?.Invoke(ex);

                log.LogFatal(ex, w => w
                    .WriteProperty("action", "ParseEvent")
                    .WriteProperty("state", "Failed")
                    .WriteProperty("eventId", input.Data.EventId.ToString())
                    .WriteProperty("eventNumber", input.EventNumber));

                return null;
            }
        }
    }
}
