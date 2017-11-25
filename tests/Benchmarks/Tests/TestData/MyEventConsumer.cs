// ==========================================================================
//  MyEventConsumer.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Squidex.Infrastructure.CQRS.Events;
using Squidex.Infrastructure.Tasks;

namespace Benchmarks.Tests.TestData
{
    public sealed class MyEventConsumer : IEventConsumer
    {
        private readonly TaskCompletionSource<object> completion = new TaskCompletionSource<object>();
        private readonly int numEvents;

        public List<int> EventNumbers { get; } = new List<int>();

        public string Name
        {
            get { return typeof(MyEventConsumer).Name; }
        }

        public string EventsFilter
        {
            get { return string.Empty; }
        }

        public MyEventConsumer(int numEvents)
        {
            this.numEvents = numEvents;
        }

        public Task ClearAsync()
        {
            return TaskHelper.Done;
        }

        public void WaitAndVerify()
        {
            completion.Task.Wait();

            if (EventNumbers.Count != numEvents)
            {
                throw new InvalidOperationException($"{EventNumbers.Count} Events have been handled");
            }

            for (var i = 0; i < EventNumbers.Count; i++)
            {
                var value = EventNumbers[i];

                if (value != i + 1)
                {
                    throw new InvalidOperationException($"Event[{i}] != value");
                }
            }
        }

        public Task On(Envelope<IEvent> @event)
        {
            if (@event.Payload is MyEvent myEvent)
            {
                EventNumbers.Add(myEvent.EventNumber);

                if (myEvent.EventNumber == numEvents)
                {
                    completion.SetResult(true);
                }
            }

            return TaskHelper.Done;
        }
    }
}