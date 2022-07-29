// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Globalization;
using Xunit;

namespace Squidex.Infrastructure.EventSourcing
{
    public sealed class RecentEventsTests
    {
        [Fact]
        public void Should_add_events()
        {
            var event1 = CreateEvent(0);
            var event2 = CreateEvent(0);
            var event3 = CreateEvent(0);

            var sut = new RecentEvents(3);

            sut.Add(event1.Id, event1.Position);
            sut.Add(event2.Id, event2.Position);
            sut.Add(event3.Id, event3.Position);

            Assert.Equal(sut.EventQueue.ToArray(), new[] { event1, event2, event3 });
        }

        [Fact]
        public void Should_remove_old_events_when_capacity_reached()
        {
            var event1 = CreateEvent(0);
            var event2 = CreateEvent(0);
            var event3 = CreateEvent(0);

            var sut = new RecentEvents(2);

            sut.Add(event1.Id, event1.Position);
            sut.Add(event2.Id, event2.Position);
            sut.Add(event3.Id, event3.Position);

            Assert.Equal(sut.EventQueue.ToArray(), new[] { event2, event3 });
        }

        [Fact]
        public void Should_not_add_events_twice()
        {
            var event1 = CreateEvent(0);
            var event2 = CreateEvent(0);

            var sut = new RecentEvents(2);

            var added1 = sut.Add(event1.Id, event1.Position);
            var added2 = sut.Add(event2.Id, event2.Position);
            var added3 = sut.Add(event2.Id, event2.Position);

            Assert.Equal(sut.EventQueue.ToArray(), new[] { event1, event2 });
            Assert.True(added1);
            Assert.True(added2);
            Assert.False(added3);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(10)]
        [InlineData(50)]
        [InlineData(100)]
        public void Should_serialize_and_deserialize(int count)
        {
            var source = new RecentEvents();

            for (var i = 0; i < count; i++)
            {
                var @event = CreateEvent(i);

                source.Add(@event.Id, @event.Position);
            }

            var serialized = RecentEvents.Parse(source.ToString());

            Assert.Equal(source.EventQueue.ToArray(), serialized.EventQueue.ToArray());
        }

        private static (Guid Id, string Position) CreateEvent(int position)
        {
            return (Guid.NewGuid(), position.ToString(CultureInfo.InvariantCulture));
        }
    }
}
