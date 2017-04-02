// ==========================================================================
//  QueryEventsBlock.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Squidex.Infrastructure.Log;

// ReSharper disable InvertIf

namespace Squidex.Infrastructure.CQRS.Events.Internal
{
    internal sealed class QueryEventsBlock : EventReceiverBlock<object, object>
    {
        private readonly IEventStore eventStore;
        private bool isStarted;
        private long handled;

        public Func<StoredEvent, Task> OnEvent { get; set; }

        public Action OnReset { get; set; }

        public QueryEventsBlock(IEventConsumer eventConsumer, IEventConsumerInfoRepository eventConsumerInfoRepository, ISemanticLog log, IEventStore eventStore) 
            : base(false, eventConsumer, eventConsumerInfoRepository, log)
        {
            this.eventStore = eventStore;
        }

        protected override async Task<object> On(object input)
        {
            if (!isStarted)
            {
                await EventConsumerInfoRepository.CreateAsync(EventConsumer.Name);

                isStarted = true;
            }

            var status = await EventConsumerInfoRepository.FindAsync(EventConsumer.Name);

            var lastReceivedEventNumber = status.LastHandledEventNumber;

            if (status.IsResetting)
            {
                await ResetAsync();
            }

            if (!status.IsStopped)
            {
                var ct = CancellationToken.None;

                await eventStore.GetEventsAsync(storedEvent => OnEvent?.Invoke(storedEvent), ct, null, lastReceivedEventNumber);
            }

            return null;
        }

        private async Task ResetAsync()
        {
            var consumerName = EventConsumer.Name;

            var actionId = Guid.NewGuid().ToString();
            try
            {
                Log.LogInformation(w => w
                    .WriteProperty("action", "EventConsumerReset")
                    .WriteProperty("actionId", actionId)
                    .WriteProperty("state", "Started")
                    .WriteProperty("eventConsumer", consumerName));

                await EventConsumer.ClearAsync();
                await EventConsumerInfoRepository.SetLastHandledEventNumberAsync(consumerName, -1);

                Log.LogInformation(w => w
                    .WriteProperty("action", "EventConsumerReset")
                    .WriteProperty("actionId", actionId)
                    .WriteProperty("state", "Completed")
                    .WriteProperty("eventConsumer", consumerName));

                OnReset?.Invoke();
            }
            catch (Exception ex)
            {
                Log.LogFatal(ex, w => w
                    .WriteProperty("action", "EventConsumerReset")
                    .WriteProperty("actionId", actionId)
                    .WriteProperty("state", "Completed")
                    .WriteProperty("eventConsumer", consumerName));
            }
        }

        protected override long GetEventNumber(object input)
        {
            return handled++;
        }
    }
}
