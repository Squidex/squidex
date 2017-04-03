// ==========================================================================
//  EventReceiver.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Threading;
using System.Threading.Tasks;
using Squidex.Infrastructure.CQRS.Events.Internal;
using Squidex.Infrastructure.Log;

// ReSharper disable InvertIf
// ReSharper disable UseObjectOrCollectionInitializer

namespace Squidex.Infrastructure.CQRS.Events
{
    public sealed class EventReceiver : DisposableObjectBase
    {
        private readonly EventDataFormatter formatter;
        private readonly IEventStore eventStore;
        private readonly IEventNotifier eventNotifier;
        private readonly IEventConsumerInfoRepository eventConsumerInfoRepository;
        private readonly ISemanticLog log;
        private QueryEventsBlock queryEventsBlock;
        private DispatchEventBlock dispatchEventBlock;
        private UpdateStateBlock updateStateBlock;
        private ParseEventBlock parseEventBlock;
        private Timer timer;

        public EventReceiver(
            EventDataFormatter formatter,
            IEventStore eventStore, 
            IEventNotifier eventNotifier,
            IEventConsumerInfoRepository eventConsumerInfoRepository,
            ISemanticLog log)
        {
            Guard.NotNull(log, nameof(log));
            Guard.NotNull(formatter, nameof(formatter));
            Guard.NotNull(eventStore, nameof(eventStore));
            Guard.NotNull(eventNotifier, nameof(eventNotifier));
            Guard.NotNull(eventConsumerInfoRepository, nameof(eventConsumerInfoRepository));

            this.log = log;
            this.formatter = formatter;
            this.eventStore = eventStore;
            this.eventNotifier = eventNotifier;
            this.eventConsumerInfoRepository = eventConsumerInfoRepository;
        }

        protected override void DisposeObject(bool disposing)
        {
            if (disposing)
            {
                try
                {
                    queryEventsBlock?.Complete();

                    timer?.Dispose();

                    updateStateBlock?.Completion.Wait();
                }
                catch (Exception ex)
                {
                    log.LogWarning(ex, w => w
                        .WriteProperty("action", "DisposeEventReceiver")
                        .WriteProperty("state", "Failed"));
                }
            }
        }

        public void Next()
        {
            queryEventsBlock.NextOrThrowAway(null);
        }

        public void Subscribe(IEventConsumer eventConsumer, int delay = 5000, bool autoTrigger = true)
        {
            Guard.NotNull(eventConsumer, nameof(eventConsumer));

            if (updateStateBlock != null)
            {
                return;
            }

            updateStateBlock = new UpdateStateBlock(eventConsumer, eventConsumerInfoRepository, log);

            dispatchEventBlock = new DispatchEventBlock(eventConsumer, eventConsumerInfoRepository, log);
            dispatchEventBlock.LinkTo(updateStateBlock.Target);

            parseEventBlock = new ParseEventBlock(eventConsumer, eventConsumerInfoRepository, log, formatter);
            parseEventBlock.LinkTo(dispatchEventBlock.Target);

            queryEventsBlock = new QueryEventsBlock(eventConsumer, eventConsumerInfoRepository, log, eventStore);
            queryEventsBlock.OnEvent = parseEventBlock.NextAsync;
            queryEventsBlock.OnReset = Reset;
            queryEventsBlock.Completion.ContinueWith(x => parseEventBlock.Complete());

            if (autoTrigger)
            {
                timer = new Timer(x => queryEventsBlock.NextOrThrowAway(null), null, 0, delay);
            }

            eventNotifier.Subscribe(() => queryEventsBlock.NextOrThrowAway(null));
        }

        private void Reset()
        {
            dispatchEventBlock.Reset();
            parseEventBlock.Reset();
            queryEventsBlock.Reset();
            updateStateBlock.Reset();
        }
    }
}