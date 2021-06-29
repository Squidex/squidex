// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Threading.Tasks;
using FakeItEasy;
using Squidex.Assets;
using Squidex.Domain.Apps.Events.Assets;
using Squidex.Infrastructure;
using Squidex.Infrastructure.EventSourcing;
using Squidex.Infrastructure.Reflection;
using Xunit;

namespace Squidex.Domain.Apps.Entities.Assets
{
    public class AssetPermanentDeleterTests
    {
        private readonly IAssetFileStore assetFiletore = A.Fake<IAssetFileStore>();
        private readonly NamedId<DomainId> appId = NamedId.Of(DomainId.NewGuid(), "my-app");
        private readonly AssetPermanentDeleter sut;

        public AssetPermanentDeleterTests()
        {
            var typeNameRegistry = new TypeNameRegistry().Map(typeof(AssetDeleted));

            sut = new AssetPermanentDeleter(assetFiletore, typeNameRegistry);
        }

        [Fact]
        public void Should_return_assets_filter_for_events_filter()
        {
            IEventConsumer consumer = sut;

            Assert.Equal("^asset-", consumer.EventsFilter);
        }

        [Fact]
        public async Task Should_do_nothing_on_clear()
        {
            IEventConsumer consumer = sut;

            await consumer.ClearAsync();
        }

        [Fact]
        public void Should_return_type_name_for_name()
        {
            IEventConsumer consumer = sut;

            Assert.Equal(nameof(AssetPermanentDeleter), consumer.Name);
        }

        [Fact]
        public async Task Should_not_delete_assets_if_event_restored()
        {
            var @event = new AssetDeleted { AppId = appId, AssetId = DomainId.NewGuid() };

            await sut.On(Envelope.Create(@event).SetRestored());

            A.CallTo(() => assetFiletore.DeleteAsync(appId.Id, @event.AssetId, A<long>._, null))
                .MustNotHaveHappened();
        }

        [Fact]
        public async Task Should_delete_assets_for_all_versions()
        {
            var @event = new AssetDeleted { AppId = appId, AssetId = DomainId.NewGuid() };

            await sut.On(Envelope.Create(@event).SetEventStreamNumber(2));

            A.CallTo(() => assetFiletore.DeleteAsync(appId.Id, @event.AssetId, 0, null))
                .MustHaveHappened();

            A.CallTo(() => assetFiletore.DeleteAsync(appId.Id, @event.AssetId, 1, null))
                .MustHaveHappened();
        }

        [Fact]
        public async Task Should_ignore_not_found_assets()
        {
            var @event = new AssetDeleted { AppId = appId, AssetId = DomainId.NewGuid() };

            A.CallTo(() => assetFiletore.DeleteAsync(appId.Id, @event.AssetId, 0, null))
                .Throws(new AssetNotFoundException("fileName"));

            await sut.On(Envelope.Create(@event).SetEventStreamNumber(2));

            A.CallTo(() => assetFiletore.DeleteAsync(appId.Id, @event.AssetId, 1, null))
                .MustHaveHappened();
        }

        [Fact]
        public async Task Should_not_ignore_exceptions()
        {
            var @event = new AssetDeleted { AppId = appId, AssetId = DomainId.NewGuid() };

            A.CallTo(() => assetFiletore.DeleteAsync(appId.Id, @event.AssetId, 0, null))
                .Throws(new InvalidOperationException());

            await Assert.ThrowsAsync<InvalidOperationException>(() => sut.On(Envelope.Create(@event).SetEventStreamNumber(2)));

            A.CallTo(() => assetFiletore.DeleteAsync(appId.Id, @event.AssetId, 1, null))
                .MustNotHaveHappened();
        }
    }
}
