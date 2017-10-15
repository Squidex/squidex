// ==========================================================================
//  ContentVersionLoaderTests.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FakeItEasy;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Events.Contents;
using Squidex.Infrastructure;
using Squidex.Infrastructure.CQRS.Events;
using Xunit;

namespace Squidex.Domain.Apps.Write.Contents
{
    public class ContentVersionLoaderTests
    {
        private readonly IEventStore eventStore = A.Fake<IEventStore>();
        private readonly IStreamNameResolver nameResolver = A.Fake<IStreamNameResolver>();
        private readonly EventDataFormatter formatter = A.Fake<EventDataFormatter>();
        private readonly Guid id = Guid.NewGuid();
        private readonly Guid appId = Guid.NewGuid();
        private readonly string streamName = Guid.NewGuid().ToString();
        private readonly ContentVersionLoader sut;

        public ContentVersionLoaderTests()
        {
            A.CallTo(() => nameResolver.GetStreamName(typeof(ContentDomainObject), id))
                .Returns(streamName);

            sut = new ContentVersionLoader(eventStore, nameResolver, formatter);
        }

        [Fact]
        public async Task Should_throw_exception_when_event_store_returns_no_events()
        {
            A.CallTo(() => eventStore.GetEventsAsync(streamName))
                .Returns(new List<StoredEvent>());

            await Assert.ThrowsAsync<DomainObjectNotFoundException>(() => sut.LoadAsync(appId, id, -1));
        }

        [Fact]
        public async Task Should_throw_exception_when_version_not_found()
        {
            A.CallTo(() => eventStore.GetEventsAsync(streamName))
                .Returns(new List<StoredEvent>());

            await Assert.ThrowsAsync<DomainObjectNotFoundException>(() => sut.LoadAsync(appId, id, 3));
        }

        [Fact]
        public async Task Should_throw_exception_when_content_is_from_another_event()
        {
            var eventData1 = new EventData();

            var event1 = new ContentCreated { Data = new NamedContentData(), AppId = new NamedId<Guid>(Guid.NewGuid(), "my-app") };

            var events = new List<StoredEvent>
            {
                new StoredEvent("0", 0, eventData1)
            };

            A.CallTo(() => eventStore.GetEventsAsync(streamName))
                .Returns(events);

            A.CallTo(() => formatter.Parse(eventData1, true))
                .Returns(new Envelope<IEvent>(event1));

            await Assert.ThrowsAsync<DomainObjectNotFoundException>(() => sut.LoadAsync(appId, id, 0));
        }

        [Fact]
        public async Task Should_load_content_from_created_event()
        {
            var eventData1 = new EventData();
            var eventData2 = new EventData();

            var event1 = new ContentCreated { Data = new NamedContentData(), AppId = new NamedId<Guid>(appId, "my-app") };
            var event2 = new ContentStatusChanged();

            var events = new List<StoredEvent>
            {
                new StoredEvent("0", 0, eventData1),
                new StoredEvent("1", 1, eventData2)
            };

            A.CallTo(() => eventStore.GetEventsAsync(streamName))
                .Returns(events);

            A.CallTo(() => formatter.Parse(eventData1, true))
                .Returns(new Envelope<IEvent>(event1));
            A.CallTo(() => formatter.Parse(eventData2, true))
                .Returns(new Envelope<IEvent>(event2));

            var data = await sut.LoadAsync(appId, id, 3);

            Assert.Same(event1.Data, data);
        }

        [Fact]
        public async Task Should_load_content_from_correct_version()
        {
            var eventData1 = new EventData();
            var eventData2 = new EventData();
            var eventData3 = new EventData();

            var event1 = new ContentCreated { Data = new NamedContentData(), AppId = new NamedId<Guid>(appId, "my-app") };
            var event2 = new ContentUpdated { Data = new NamedContentData() };
            var event3 = new ContentUpdated { Data = new NamedContentData() };

            var events = new List<StoredEvent>
            {
                new StoredEvent("0", 0, eventData1),
                new StoredEvent("1", 1, eventData2),
                new StoredEvent("2", 2, eventData3)
            };

            A.CallTo(() => eventStore.GetEventsAsync(streamName))
                .Returns(events);

            A.CallTo(() => formatter.Parse(eventData1, true))
                .Returns(new Envelope<IEvent>(event1));
            A.CallTo(() => formatter.Parse(eventData2, true))
                .Returns(new Envelope<IEvent>(event2));
            A.CallTo(() => formatter.Parse(eventData3, true))
                .Returns(new Envelope<IEvent>(event3));

            var data = await sut.LoadAsync(appId, id, 1);

            Assert.Equal(event2.Data, data);
        }
    }
}
