// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Threading.Tasks;
using FakeItEasy;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using NodaTime;
using Squidex.Domain.Apps.Core.HandleRules;
using Squidex.Domain.Apps.Core.Rules.EnrichedEvents;
using Squidex.Domain.Apps.Events;
using Squidex.Domain.Apps.Events.Contents;
using Squidex.Infrastructure;
using Squidex.Infrastructure.EventSourcing;
using Squidex.Shared.Users;
using Xunit;

namespace Squidex.Domain.Apps.Core.Operations.HandleRules
{
    public class EventEnricherTests
    {
        private readonly IUserResolver userResolver = A.Fake<IUserResolver>();
        private readonly EventEnricher sut;

        public EventEnricherTests()
        {
            var cache = new MemoryCache(Options.Create(new MemoryCacheOptions()));

            sut = new EventEnricher(cache, userResolver);
        }

        [Fact]
        public async Task Should_enrich_with_timestamp()
        {
            var timestamp = SystemClock.Instance.GetCurrentInstant().WithoutMs();

            var @event =
                Envelope.Create<AppEvent>(new ContentCreated())
                    .SetTimestamp(timestamp);

            var enrichedEvent = new EnrichedContentEvent();

            await sut.EnrichAsync(enrichedEvent, @event);

            Assert.Equal(timestamp, enrichedEvent.Timestamp);
        }

        [Fact]
        public async Task Should_enrich_with_appId()
        {
            var appId = NamedId.Of(DomainId.NewGuid(), "my-app");

            var @event =
                Envelope.Create<AppEvent>(new ContentCreated
                {
                    AppId = appId
                });

            var enrichedEvent = new EnrichedContentEvent();

            await sut.EnrichAsync(enrichedEvent, @event);

            Assert.Equal(appId, enrichedEvent.AppId);
        }

        [Fact]
        public async Task Should_not_enrich_with_user_if_token_is_null()
        {
            RefToken actor = null!;

            var @event =
                Envelope.Create<AppEvent>(new ContentCreated
                {
                    Actor = actor
                });

            var enrichedEvent = new EnrichedContentEvent();

            await sut.EnrichAsync(enrichedEvent, @event);

            Assert.Null(enrichedEvent.User);

            A.CallTo(() => userResolver.FindByIdAsync(A<string>._, default))
                .MustNotHaveHappened();
        }

        [Fact]
        public async Task Should_enrich_with_user()
        {
            var actor = RefToken.Client("me");

            var user = A.Dummy<IUser>();

            A.CallTo(() => userResolver.FindByIdAsync(actor.Identifier, default))
                .Returns(user);

            var @event =
                Envelope.Create<AppEvent>(new ContentCreated
                {
                    Actor = actor
                });

            var enrichedEvent = new EnrichedContentEvent();

            await sut.EnrichAsync(enrichedEvent, @event);

            Assert.Equal(user, enrichedEvent.User);

            A.CallTo(() => userResolver.FindByIdAsync(A<string>._, default))
                .MustHaveHappenedOnceExactly();
        }

        [Fact]
        public async Task Should_enrich_with_user_and_cache()
        {
            var actor = RefToken.Client("me");

            var user = A.Dummy<IUser>();

            A.CallTo(() => userResolver.FindByIdAsync(actor.Identifier, default))
                .Returns(user);

            var event1 =
                Envelope.Create<AppEvent>(new ContentCreated
                {
                    Actor = actor
                });

            var event2 =
                Envelope.Create<AppEvent>(new ContentCreated
                {
                    Actor = actor
                });

            var enrichedEvent1 = new EnrichedContentEvent();
            var enrichedEvent2 = new EnrichedContentEvent();

            await sut.EnrichAsync(enrichedEvent1, event1);
            await sut.EnrichAsync(enrichedEvent2, event2);

            Assert.Equal(user, enrichedEvent1.User);
            Assert.Equal(user, enrichedEvent2.User);

            A.CallTo(() => userResolver.FindByIdAsync(A<string>._, default))
                .MustHaveHappenedOnceExactly();
        }
    }
}
