// ==========================================================================
//  DispatchingTests.cs
//  PinkParrot Headless CMS
// ==========================================================================
//  Copyright (c) PinkParrot Group
//  All rights reserved.
// ==========================================================================

using System.Threading.Tasks;
using PinkParrot.Infrastructure.Dispatching;
using PinkParrot.Infrastructure.Tasks;
using Xunit;

namespace PinkParrot.Infrastructure
{
    public sealed class DispatchingTests
    {
        private interface IEvent { }

        private class EventA : IEvent { }
        private class EventB : IEvent { }
        private class Unknown : IEvent { }

        private class AsyncFuncConsumer
        {
            public int EventATriggered { get; private set; }
            public int EventBTriggered { get; private set; }

            public Task<int> DispatchEventAsync(IEvent @event)
            {
                return this.DispatchFuncAsync(@event, 9);
            }

            public Task<int> DispatchEventAsync(IEvent @event, int context)
            {
                return this.DispatchFuncAsync(@event, context, 13);
            }

            public Task<int> On(EventA @event)
            {
                return Task.FromResult(++EventATriggered);
            }

            public Task<int> On(EventB @event)
            {
                return Task.FromResult(++EventBTriggered);
            }

            public Task<int> On(EventA @event,  int context)
            {
                return Task.FromResult(++EventATriggered + context);
            }

            public Task<int> On(EventB @event, int context)
            {
                return Task.FromResult(++EventBTriggered + context);
            }
        }

        private class AsyncConsumer
        {
            public int EventATriggered { get; private set; }
            public int EventBTriggered { get; private set; }

            public Task<bool> DispatchEventAsync(IEvent @event)
            {
                return this.DispatchActionAsync(@event);
            }

            public Task<bool> DispatchEventAsync(IEvent @event, int context)
            {
                return this.DispatchActionAsync(@event, context);
            }

            public Task On(EventA @event)
            {
                EventATriggered++;
                return TaskHelper.Done;
            }

            public Task On(EventB @event)
            {
                EventBTriggered++;
                return TaskHelper.Done;
            }

            public Task On(EventA @event, int context)
            {
                EventATriggered = EventATriggered + context;
                return TaskHelper.Done;
            }

            public Task On(EventB @event, int context)
            {
                EventBTriggered = EventATriggered + context;
                return TaskHelper.Done;
            }
        }

        private class SyncFuncConsumer
        {
            public int EventATriggered { get; private set; }
            public int EventBTriggered { get; private set; }

            public int DispatchEvent(IEvent @event)
            {
                return this.DispatchFunc(@event, 9);
            }

            public int DispatchEvent(IEvent @event, int context)
            {
                return this.DispatchFunc(@event, context, 13);
            }

            public int On(EventA @event)
            {
                return ++EventATriggered;
            }

            public int On(EventB @event)
            {
                return ++EventBTriggered;
            }

            public int On(EventA @event, int context)
            {
                return ++EventATriggered + context;
            }

            public int On(EventB @event, int context)
            {
                return ++EventBTriggered + context;
            }
        }

        private class SyncActionConsumer
        {
            public int EventATriggered { get; private set; }
            public int EventBTriggered { get; private set; }

            public bool DispatchEvent(IEvent @event)
            {
                return this.DispatchAction(@event);
            }

            public bool DispatchEvent(IEvent @event, int context)
            {
                return this.DispatchAction(@event, context);
            }

            public void On(EventA @event)
            {
                EventATriggered++;
            }

            public void On(EventB @event)
            {
                EventBTriggered++;
            }

            public void On(EventA @event, int context)
            {
                EventATriggered = EventATriggered + context;
            }

            public void On(EventB @event, int context)
            {
                EventBTriggered = EventATriggered + context;
            }
        }

        [Fact]
        public void Should_invoke_correct_event()
        {
            var consumer = new SyncActionConsumer();

            consumer.DispatchEvent(new EventA());
            consumer.DispatchEvent(new EventB());
            consumer.DispatchEvent(new EventB());
            consumer.DispatchEvent(new Unknown());

            Assert.Equal(1, consumer.EventATriggered);
            Assert.Equal(2, consumer.EventBTriggered);
        }

        [Fact]
        public void Should_invoke_correct_event_with_context()
        {
            var consumer = new SyncActionConsumer();

            consumer.DispatchEvent(new EventA(), 2);
            consumer.DispatchEvent(new EventB(), 2);
            consumer.DispatchEvent(new EventB(), 2);
            consumer.DispatchEvent(new Unknown(), 2);

            Assert.Equal(2, consumer.EventATriggered);
            Assert.Equal(4, consumer.EventBTriggered);
        }

        [Fact]
        public async Task Should_invoke_correct_event_asynchronously()
        {
            var consumer = new AsyncConsumer();

            await consumer.DispatchEventAsync(new EventA());
            await consumer.DispatchEventAsync(new EventB());
            await consumer.DispatchEventAsync(new EventB());
            await consumer.DispatchEventAsync(new Unknown());

            Assert.Equal(1, consumer.EventATriggered);
            Assert.Equal(2, consumer.EventBTriggered);
        }

        [Fact]
        public async Task Should_invoke_correct_event_with_context_asynchronously()
        {
            var consumer = new AsyncConsumer();

            await consumer.DispatchEventAsync(new EventA(), 2);
            await consumer.DispatchEventAsync(new EventB(), 2);
            await consumer.DispatchEventAsync(new EventB(), 2);
            await consumer.DispatchEventAsync(new Unknown(), 2);

            Assert.Equal(2, consumer.EventATriggered);
            Assert.Equal(4, consumer.EventBTriggered);
        }

        [Fact]
        public void Should_invoke_correct_event_and_return()
        {
            var consumer = new SyncFuncConsumer();

            Assert.Equal(1, consumer.DispatchEvent(new EventA()));
            Assert.Equal(1, consumer.DispatchEvent(new EventB()));
            Assert.Equal(2, consumer.DispatchEvent(new EventB()));
            Assert.Equal(9, consumer.DispatchEvent(new Unknown()));

            Assert.Equal(1, consumer.EventATriggered);
            Assert.Equal(2, consumer.EventBTriggered);
        }

        [Fact]
        public void Should_invoke_correct_event_with_context_and_return()
        {
            var consumer = new SyncFuncConsumer();

            Assert.Equal(11, consumer.DispatchEvent(new EventA(), 10));
            Assert.Equal(11, consumer.DispatchEvent(new EventB(), 10));
            Assert.Equal(12, consumer.DispatchEvent(new EventB(), 10));
            Assert.Equal(13, consumer.DispatchEvent(new Unknown(), 10));

            Assert.Equal(1, consumer.EventATriggered);
            Assert.Equal(2, consumer.EventBTriggered);
        }

        [Fact]
        public async Task Should_invoke_correct_event_and_return_synchronously()
        {
            var consumer = new AsyncFuncConsumer();

            Assert.Equal(1, await consumer.DispatchEventAsync(new EventA()));
            Assert.Equal(1, await consumer.DispatchEventAsync(new EventB()));
            Assert.Equal(2, await consumer.DispatchEventAsync(new EventB()));
            Assert.Equal(9, await consumer.DispatchEventAsync(new Unknown()));

            Assert.Equal(1, consumer.EventATriggered);
            Assert.Equal(2, consumer.EventBTriggered);
        }

        [Fact]
        public async Task Should_invoke_correct_event_with_context_and_return_synchronously()
        {
            var consumer = new AsyncFuncConsumer();

            Assert.Equal(11, await consumer.DispatchEventAsync(new EventA(), 10));
            Assert.Equal(11, await consumer.DispatchEventAsync(new EventB(), 10));
            Assert.Equal(12, await consumer.DispatchEventAsync(new EventB(), 10));
            Assert.Equal(13, await consumer.DispatchEventAsync(new Unknown(), 10));

            Assert.Equal(1, consumer.EventATriggered);
            Assert.Equal(2, consumer.EventBTriggered);
        }
    }
}
