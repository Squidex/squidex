﻿// ==========================================================================
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
        private readonly TypeNameRegistry typeNameRegistry;
        private readonly AssetPermanentDeleter sut;

        public AssetPermanentDeleterTests()
        {
            typeNameRegistry =
                new TypeNameRegistry()
                    .Map(typeof(AssetCreated))
                    .Map(typeof(AssetDeleted));

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
        public void Should_handle_deletion_event()
        {
            var storedEvent =
                new StoredEvent("stream", "1", 1,
                    new EventData(typeNameRegistry.GetName<AssetDeleted>(), new EnvelopeHeaders(), "payload"));

            Assert.True(sut.Handles(storedEvent));
        }

        [Fact]
        public void Should_not_handle_creation_event()
        {
            var storedEvent =
                new StoredEvent("stream", "1", 1,
                    new EventData(typeNameRegistry.GetName<AssetCreated>(), new EnvelopeHeaders(), "payload"));

            Assert.False(sut.Handles(storedEvent));
        }

        [Fact]
        public async Task Should_not_delete_assets_if_event_restored()
        {
            var @event = new AssetDeleted { AppId = appId, AssetId = DomainId.NewGuid() };

            await sut.On(Envelope.Create(@event).SetRestored());

            A.CallTo(() => assetFiletore.DeleteAsync(appId.Id, @event.AssetId, default))
                .MustNotHaveHappened();
        }

        [Fact]
        public async Task Should_delete_asset()
        {
            var @event = new AssetDeleted { AppId = appId, AssetId = DomainId.NewGuid() };

            await sut.On(Envelope.Create(@event));

            A.CallTo(() => assetFiletore.DeleteAsync(appId.Id, @event.AssetId, default))
                .MustHaveHappened();
        }

        [Fact]
        public async Task Should_ignore_not_found_assets()
        {
            var @event = new AssetDeleted { AppId = appId, AssetId = DomainId.NewGuid() };

            A.CallTo(() => assetFiletore.DeleteAsync(appId.Id, @event.AssetId, default))
                .Throws(new AssetNotFoundException("fileName"));

            await sut.On(Envelope.Create(@event));
        }

        [Fact]
        public async Task Should_not_ignore_exceptions()
        {
            var @event = new AssetDeleted { AppId = appId, AssetId = DomainId.NewGuid() };

            A.CallTo(() => assetFiletore.DeleteAsync(appId.Id, @event.AssetId, default))
                .Throws(new InvalidOperationException());

            await Assert.ThrowsAsync<InvalidOperationException>(() => sut.On(Envelope.Create(@event)));
        }
    }
}
