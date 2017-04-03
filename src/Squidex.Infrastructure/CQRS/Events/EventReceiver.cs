// ==========================================================================
//  EventReceiver.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Threading;
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
        private IEventReceiverBlock[] blocks;
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
            queryEventsBlock.NextOrThrowAway();
        }

        public void Subscribe(IEventConsumer eventConsumer, int delay = 5000, bool autoTrigger = true)
        {
            Guard.NotNull(eventConsumer, nameof(eventConsumer));

            if (updateStateBlock != null)
            {
                return;
            }

            var onError = new Action<Exception>(ex => Stop(ex, eventConsumer));

            updateStateBlock = new UpdateStateBlock(eventConsumerInfoRepository, eventConsumer, log);
            updateStateBlock.OnError = onError;

            dispatchEventBlock = new DispatchEventBlock(eventConsumer, log);
            dispatchEventBlock.OnError = onError;
            dispatchEventBlock.LinkTo(updateStateBlock.Target);

            parseEventBlock = new ParseEventBlock(formatter, log);
            parseEventBlock.OnError = onError;
            parseEventBlock.LinkTo(dispatchEventBlock.Target);

            queryEventsBlock = new QueryEventsBlock(eventConsumerInfoRepository, eventConsumer, eventStore, log);
            queryEventsBlock.OnEvent = parseEventBlock.NextAsync;
            queryEventsBlock.OnReset = Reset;
            queryEventsBlock.OnError = onError;
            queryEventsBlock.Completion.ContinueWith(x => parseEventBlock.Complete());

            blocks = new IEventReceiverBlock[] { updateStateBlock, dispatchEventBlock, parseEventBlock, queryEventsBlock };

            if (autoTrigger)
            {
                timer = new Timer(x => queryEventsBlock.NextOrThrowAway(), null, 0, delay);
            }

            eventNotifier.Subscribe(() => queryEventsBlock.NextOrThrowAway());
        }

        private void Stop(Exception ex, IEventConsumer eventConsumer)
        {
            foreach (var block in blocks)
            {
                block.Stop();
            }

            try
            {
                eventConsumerInfoRepository.StopAsync(eventConsumer.Name, ex.ToString()).Wait();
            }
            catch (Exception ex2)
            {
                log.LogFatal(ex2, w => w
                    .WriteProperty("action", "StopConsumer")
                    .WriteProperty("state", "Failed"));
            }
        }

        private void Reset()
        {
            foreach (var block in blocks)
            {
                block.Reset();
            }
        }
    }
}