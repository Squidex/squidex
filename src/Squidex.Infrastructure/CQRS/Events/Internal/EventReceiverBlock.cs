// ==========================================================================
//  EventReceiverBlock.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Threading.Tasks;
using Squidex.Infrastructure.Log;
using System.Threading.Tasks.Dataflow;

namespace Squidex.Infrastructure.CQRS.Events.Internal
{
    public abstract class EventReceiverBlock<TInput, TOutput>
    {
        private long lastEventNumber = -1;

        protected ISemanticLog Log { get; }

        protected IEventConsumer EventConsumer { get; }

        protected IEventConsumerInfoRepository EventConsumerInfoRepository { get; }
        
        public ITargetBlock<TInput> Target { get; }

        public Task Completion
        {
            get { return Target.Completion; }
        }

        protected EventReceiverBlock(bool transform, IEventConsumer eventConsumer, IEventConsumerInfoRepository eventConsumerInfoRepository, ISemanticLog log)
        {
            EventConsumer = eventConsumer;
            EventConsumerInfoRepository = eventConsumerInfoRepository;

            Log = log;

            if (transform)
            {
                var nullHandlerBlock =
                    new ActionBlock<TOutput>(_ => { });

                var transformBlock =
                    new TransformBlock<TInput, TOutput>(new Func<TInput, Task<TOutput>>(HandleAsync),
                        new ExecutionDataflowBlockOptions { BoundedCapacity = 1 });
                transformBlock.LinkTo(nullHandlerBlock, new DataflowLinkOptions { PropagateCompletion = true }, x => x == null);

                Target = transformBlock;
            }
            else
            {
                Target =
                    new ActionBlock<TInput>(HandleAsync,
                        new ExecutionDataflowBlockOptions { BoundedCapacity = 1 });
            }
        }

        public Task NextAsync(TInput input)
        {
            return Target.SendAsync(input);
        }

        public void NextOrThrowAway(TInput input)
        {
            Target.Post(input);
        }

        public void Complete()
        {
            Target.Complete();
        }

        public void Reset()
        {
            lastEventNumber = -1;
        }

        public void LinkTo(ITargetBlock<TOutput> other)
        {
            if (Target is TransformBlock<TInput, TOutput> transformBlock)
            {
                transformBlock.LinkTo(other, new DataflowLinkOptions { PropagateCompletion = true }, e => e != null);
            }
        }

        protected abstract Task<TOutput> On(TInput input);

        protected abstract long GetEventNumber(TInput input);

        private async Task<TOutput> HandleAsync(TInput input)
        {
            try
            {
                var eventNumber = GetEventNumber(input);

                if (eventNumber > lastEventNumber)
                {
                    var envelope = await On(input);

                    lastEventNumber = eventNumber;

                    return envelope;
                }
            }
            catch (Exception ex)
            {
                Log.LogFatal(ex, w => w.WriteProperty("action", "EventHandlingFailed"));

                try
                {
                    await EventConsumerInfoRepository.StopAsync(EventConsumer.Name, ex.ToString());
                }
                catch (Exception ex2)
                {
                    Log.LogFatal(ex2, w => w.WriteProperty("action", "EventHandlingFailed"));
                }
            }

            return default(TOutput);
        }
    }
}
