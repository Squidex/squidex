// ==========================================================================
//  DispatchingTests.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System.Threading.Tasks;
using Squidex.Infrastructure.Dispatching;
using Squidex.Infrastructure.Tasks;
using Xunit;

namespace Squidex.Infrastructure
{
    public sealed class DispatchingTests
    {
        private interface IMyEvent
        {
        }

        private class MyEventA : IMyEvent
        {
        }

        private class MyEventB : IMyEvent
        {
        }

        private class MyUnknown : IMyEvent
        {
        }

        private class MyAsyncFuncConsumer
        {
            public int EventATriggered { get; private set; }
            public int EventBTriggered { get; private set; }

            public Task<int> DispatchEventAsync(IMyEvent @event)
            {
                return this.DispatchFuncAsync(@event, 9);
            }

            public Task<int> DispatchEventAsync(IMyEvent @event, int context)
            {
                return this.DispatchFuncAsync(@event, context, 13);
            }

            public Task<int> On(MyEventA @event)
            {
                return Task.FromResult(++EventATriggered);
            }

            public Task<int> On(MyEventB @event)
            {
                return Task.FromResult(++EventBTriggered);
            }

            public Task<int> On(MyEventA @event,  int context)
            {
                return Task.FromResult(++EventATriggered + context);
            }

            public Task<int> On(MyEventB @event, int context)
            {
                return Task.FromResult(++EventBTriggered + context);
            }
        }

        private class MyAsyncConsumer
        {
            public int EventATriggered { get; private set; }
            public int EventBTriggered { get; private set; }

            public Task<bool> DispatchEventAsync(IMyEvent @event)
            {
                return this.DispatchActionAsync(@event);
            }

            public Task<bool> DispatchEventAsync(IMyEvent @event, int context)
            {
                return this.DispatchActionAsync(@event, context);
            }

            public Task On(MyEventA @event)
            {
                EventATriggered++;
                return TaskHelper.Done;
            }

            public Task On(MyEventB @event)
            {
                EventBTriggered++;
                return TaskHelper.Done;
            }

            public Task On(MyEventA @event, int context)
            {
                EventATriggered = EventATriggered + context;
                return TaskHelper.Done;
            }

            public Task On(MyEventB @event, int context)
            {
                EventBTriggered = EventATriggered + context;
                return TaskHelper.Done;
            }
        }

        private class MySyncFuncConsumer
        {
            public int EventATriggered { get; private set; }
            public int EventBTriggered { get; private set; }

            public int DispatchEvent(IMyEvent @event)
            {
                return this.DispatchFunc(@event, 9);
            }

            public int DispatchEvent(IMyEvent @event, int context)
            {
                return this.DispatchFunc(@event, context, 13);
            }

            public int On(MyEventA @event)
            {
                return ++EventATriggered;
            }

            public int On(MyEventB @event)
            {
                return ++EventBTriggered;
            }

            public int On(MyEventA @event, int context)
            {
                return ++EventATriggered + context;
            }

            public int On(MyEventB @event, int context)
            {
                return ++EventBTriggered + context;
            }
        }

        private class MySyncActionConsumer
        {
            public int EventATriggered { get; private set; }
            public int EventBTriggered { get; private set; }

            public bool DispatchEvent(IMyEvent @event)
            {
                return this.DispatchAction(@event);
            }

            public bool DispatchEvent(IMyEvent @event, int context)
            {
                return this.DispatchAction(@event, context);
            }

            public void On(MyEventA @event)
            {
                EventATriggered++;
            }

            public void On(MyEventB @event)
            {
                EventBTriggered++;
            }

            public void On(MyEventA @event, int context)
            {
                EventATriggered = EventATriggered + context;
            }

            public void On(MyEventB @event, int context)
            {
                EventBTriggered = EventATriggered + context;
            }
        }

        [Fact]
        public void Should_invoke_correct_event()
        {
            var consumer = new MySyncActionConsumer();

            consumer.DispatchEvent(new MyEventA());
            consumer.DispatchEvent(new MyEventB());
            consumer.DispatchEvent(new MyEventB());
            consumer.DispatchEvent(new MyUnknown());

            Assert.Equal(1, consumer.EventATriggered);
            Assert.Equal(2, consumer.EventBTriggered);
        }

        [Fact]
        public void Should_invoke_correct_event_with_context()
        {
            var consumer = new MySyncActionConsumer();

            consumer.DispatchEvent(new MyEventA(), 2);
            consumer.DispatchEvent(new MyEventB(), 2);
            consumer.DispatchEvent(new MyEventB(), 2);
            consumer.DispatchEvent(new MyUnknown(), 2);

            Assert.Equal(2, consumer.EventATriggered);
            Assert.Equal(4, consumer.EventBTriggered);
        }

        [Fact]
        public async Task Should_invoke_correct_event_asynchronously()
        {
            var consumer = new MyAsyncConsumer();

            await consumer.DispatchEventAsync(new MyEventA());
            await consumer.DispatchEventAsync(new MyEventB());
            await consumer.DispatchEventAsync(new MyEventB());
            await consumer.DispatchEventAsync(new MyUnknown());

            Assert.Equal(1, consumer.EventATriggered);
            Assert.Equal(2, consumer.EventBTriggered);
        }

        [Fact]
        public async Task Should_invoke_correct_event_with_context_asynchronously()
        {
            var consumer = new MyAsyncConsumer();

            await consumer.DispatchEventAsync(new MyEventA(), 2);
            await consumer.DispatchEventAsync(new MyEventB(), 2);
            await consumer.DispatchEventAsync(new MyEventB(), 2);
            await consumer.DispatchEventAsync(new MyUnknown(), 2);

            Assert.Equal(2, consumer.EventATriggered);
            Assert.Equal(4, consumer.EventBTriggered);
        }

        [Fact]
        public void Should_invoke_correct_event_and_return()
        {
            var consumer = new MySyncFuncConsumer();

            Assert.Equal(1, consumer.DispatchEvent(new MyEventA()));
            Assert.Equal(1, consumer.DispatchEvent(new MyEventB()));
            Assert.Equal(2, consumer.DispatchEvent(new MyEventB()));
            Assert.Equal(9, consumer.DispatchEvent(new MyUnknown()));

            Assert.Equal(1, consumer.EventATriggered);
            Assert.Equal(2, consumer.EventBTriggered);
        }

        [Fact]
        public void Should_invoke_correct_event_with_context_and_return()
        {
            var consumer = new MySyncFuncConsumer();

            Assert.Equal(11, consumer.DispatchEvent(new MyEventA(), 10));
            Assert.Equal(11, consumer.DispatchEvent(new MyEventB(), 10));
            Assert.Equal(12, consumer.DispatchEvent(new MyEventB(), 10));
            Assert.Equal(13, consumer.DispatchEvent(new MyUnknown(), 10));

            Assert.Equal(1, consumer.EventATriggered);
            Assert.Equal(2, consumer.EventBTriggered);
        }

        [Fact]
        public async Task Should_invoke_correct_event_and_return_synchronously()
        {
            var consumer = new MyAsyncFuncConsumer();

            Assert.Equal(1, await consumer.DispatchEventAsync(new MyEventA()));
            Assert.Equal(1, await consumer.DispatchEventAsync(new MyEventB()));
            Assert.Equal(2, await consumer.DispatchEventAsync(new MyEventB()));
            Assert.Equal(9, await consumer.DispatchEventAsync(new MyUnknown()));

            Assert.Equal(1, consumer.EventATriggered);
            Assert.Equal(2, consumer.EventBTriggered);
        }

        [Fact]
        public async Task Should_invoke_correct_event_with_context_and_return_synchronously()
        {
            var consumer = new MyAsyncFuncConsumer();

            Assert.Equal(11, await consumer.DispatchEventAsync(new MyEventA(), 10));
            Assert.Equal(11, await consumer.DispatchEventAsync(new MyEventB(), 10));
            Assert.Equal(12, await consumer.DispatchEventAsync(new MyEventB(), 10));
            Assert.Equal(13, await consumer.DispatchEventAsync(new MyUnknown(), 10));

            Assert.Equal(1, consumer.EventATriggered);
            Assert.Equal(2, consumer.EventBTriggered);
        }
    }
}
