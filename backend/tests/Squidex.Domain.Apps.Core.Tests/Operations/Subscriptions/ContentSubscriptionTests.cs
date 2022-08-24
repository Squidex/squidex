// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Rules.EnrichedEvents;
using Squidex.Domain.Apps.Core.Subscriptions;
using Squidex.Domain.Apps.Events.Apps;
using Squidex.Domain.Apps.Events.Contents;
using Squidex.Infrastructure;
using Xunit;

namespace Squidex.Domain.Apps.Core.Operations.Subscriptions
{
    public class ContentSubscriptionTests
    {
        [Fact]
        public async Task Should_return_true_for_enriched_content_event()
        {
            var sut = new ContentSubscription();

            Assert.True(await sut.ShouldHandle(new EnrichedContentEvent()));
        }

        [Fact]
        public async Task Should_return_false_for_wrong_event()
        {
            var sut = new ContentSubscription();

            Assert.False(await sut.ShouldHandle(new AppCreated()));
        }

        [Fact]
        public async Task Should_return_true_for_content_event()
        {
            var sut = new ContentSubscription();

            Assert.True(await sut.ShouldHandle(new ContentCreated()));
        }

        [Fact]
        public async Task Should_return_true_for_content_event_with_correct_type()
        {
            var sut = new ContentSubscription { Type = EnrichedContentEventType.Created };

            Assert.True(await sut.ShouldHandle(new ContentCreated()));
        }

        [Fact]
        public async Task Should_return_false_for_content_event_with_wrong_type()
        {
            var sut = new ContentSubscription { Type = EnrichedContentEventType.Deleted };

            Assert.False(await sut.ShouldHandle(new ContentCreated()));
        }

        [Fact]
        public async Task Should_return_true_for_content_event_with_correct_schema()
        {
            var sut = new ContentSubscription { SchemaName = "my-schema" };

            var schemaId = NamedId.Of(DomainId.NewGuid(), "my-schema");

            Assert.True(await sut.ShouldHandle(new ContentCreated { SchemaId = schemaId }));
        }

        [Fact]
        public async Task Should_return_false_for_content_event_with_wrong_schema()
        {
            var sut = new ContentSubscription { SchemaName = "wrong-schema" };

            var schemaId = NamedId.Of(DomainId.NewGuid(), "my-schema");

            Assert.False(await sut.ShouldHandle(new ContentCreated { SchemaId = schemaId }));
        }
    }
}
