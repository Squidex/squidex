// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.IO;
using System.Threading.Tasks;
using FakeItEasy;
using Squidex.Domain.Apps.Events.Assets;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Assets;
using Squidex.Infrastructure.EventSourcing;
using Xunit;

namespace Squidex.Domain.Apps.Entities.Assets
{
    public class RepairFilesTests
    {
        private readonly IAssetFileStore assetFileStore = A.Fake<IAssetFileStore>();
        private readonly NamedId<DomainId> appId = NamedId.Of(DomainId.NewGuid(), "my-app");
        private readonly RepairFiles sut;

        public RepairFilesTests()
        {
            sut = new RepairFiles(assetFileStore);
        }

        [Fact]
        public void Should_return_assets_filter_for_events_filter()
        {
            IEventConsumer consumer = sut;

            Assert.Equal("^asset\\-", consumer.EventsFilter);
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

            Assert.Equal(nameof(RepairFiles), consumer.Name);
        }

        [Fact]
        public async Task Should_repair_created_asset_if_not_found()
        {
            var @event = new AssetCreated { AppId = appId, AssetId = DomainId.NewGuid() };

            A.CallTo(() => assetFileStore.GetFileSizeAsync(appId.Id, @event.AssetId, 0, default))
                .Throws(new AssetNotFoundException("file"));

            await sut.On(Envelope.Create(@event));

            A.CallTo(() => assetFileStore.UploadAsync(appId.Id, @event.AssetId, 0, A<Stream>._, default))
                .MustHaveHappened();
        }

        [Fact]
        public async Task Should_not_repair_created_asset_if_found()
        {
            var @event = new AssetCreated { AppId = appId, AssetId = DomainId.NewGuid() };

            A.CallTo(() => assetFileStore.GetFileSizeAsync(appId.Id, @event.AssetId, 0, default))
                .Returns(100);

            await sut.On(Envelope.Create(@event));

            A.CallTo(() => assetFileStore.UploadAsync(appId.Id, @event.AssetId, 0, A<Stream>._, default))
                .MustNotHaveHappened();
        }

        [Fact]
        public async Task Should_repair_updated_asset_if_not_found()
        {
            var @event = new AssetUpdated { AppId = appId, AssetId = DomainId.NewGuid(), FileVersion = 3 };

            A.CallTo(() => assetFileStore.GetFileSizeAsync(appId.Id, @event.AssetId, 3, default))
                .Throws(new AssetNotFoundException("file"));

            await sut.On(Envelope.Create(@event));

            A.CallTo(() => assetFileStore.UploadAsync(appId.Id, @event.AssetId, 3, A<Stream>._, default))
                .MustHaveHappened();
        }

        [Fact]
        public async Task Should_not_repair_updated_asset_if_found()
        {
            var @event = new AssetUpdated { AppId = appId, AssetId = DomainId.NewGuid(), FileVersion = 3 };

            A.CallTo(() => assetFileStore.GetFileSizeAsync(appId.Id, @event.AssetId, 3, default))
                .Returns(100);

            await sut.On(Envelope.Create(@event));

            A.CallTo(() => assetFileStore.UploadAsync(appId.Id, @event.AssetId, 3, A<Stream>._, default))
                .MustNotHaveHappened();
        }
    }
}
