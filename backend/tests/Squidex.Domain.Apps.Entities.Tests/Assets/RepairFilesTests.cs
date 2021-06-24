// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using FakeItEasy;
using Squidex.Assets;
using Squidex.Domain.Apps.Events.Assets;
using Squidex.Infrastructure;
using Squidex.Infrastructure.EventSourcing;
using Xunit;

namespace Squidex.Domain.Apps.Entities.Assets
{
    public class RepairFilesTests
    {
        private readonly IEventStore eventStore = A.Fake<IEventStore>();
        private readonly IEventDataFormatter eventDataFormatter = A.Fake<IEventDataFormatter>();
        private readonly IAssetFileStore assetFileStore = A.Fake<IAssetFileStore>();
        private readonly NamedId<DomainId> appId = NamedId.Of(DomainId.NewGuid(), "my-app");
        private readonly RebuildFiles sut;

        public RepairFilesTests()
        {
            sut = new RebuildFiles(assetFileStore, eventStore, eventDataFormatter);
        }

        [Fact]
        public async Task Should_repair_created_asset_if_not_found()
        {
            var @event = new AssetCreated { AppId = appId, AssetId = DomainId.NewGuid() };

            SetupEvent(@event);

            A.CallTo(() => assetFileStore.GetFileSizeAsync(appId.Id, @event.AssetId, 0, null, default))
                .Throws(new AssetNotFoundException("file"));

            await sut.RepairAsync();

            A.CallTo(() => assetFileStore.UploadAsync(appId.Id, @event.AssetId, 0, null, A<Stream>._, true, default))
                .MustHaveHappened();
        }

        [Fact]
        public async Task Should_not_repair_created_asset_if_found()
        {
            var @event = new AssetCreated { AppId = appId, AssetId = DomainId.NewGuid() };

            SetupEvent(@event);

            A.CallTo(() => assetFileStore.GetFileSizeAsync(appId.Id, @event.AssetId, 0, null, default))
                .Returns(100);

            await sut.RepairAsync();

            A.CallTo(() => assetFileStore.UploadAsync(appId.Id, @event.AssetId, 0, null, A<Stream>._, true, default))
                .MustNotHaveHappened();
        }

        [Fact]
        public async Task Should_repair_updated_asset_if_not_found()
        {
            var @event = new AssetUpdated { AppId = appId, AssetId = DomainId.NewGuid(), FileVersion = 3 };

            SetupEvent(@event);

            A.CallTo(() => assetFileStore.GetFileSizeAsync(appId.Id, @event.AssetId, 3, null, default))
                .Throws(new AssetNotFoundException("file"));

            await sut.RepairAsync();

            A.CallTo(() => assetFileStore.UploadAsync(appId.Id, @event.AssetId, 3, null, A<Stream>._, true, default))
                .MustHaveHappened();
        }

        [Fact]
        public async Task Should_not_repair_updated_asset_if_found()
        {
            var @event = new AssetUpdated { AppId = appId, AssetId = DomainId.NewGuid(), FileVersion = 3 };

            SetupEvent(@event);

            A.CallTo(() => assetFileStore.GetFileSizeAsync(appId.Id, @event.AssetId, 3, null, default))
                .Returns(100);

            await sut.RepairAsync();

            A.CallTo(() => assetFileStore.UploadAsync(appId.Id, @event.AssetId, 3, null, A<Stream>._, true, default))
                .MustNotHaveHappened();
        }

        [Fact]
        public async Task Should_ignore_old_events()
        {
            SetupEvent(null);

            await sut.RepairAsync();

            A.CallTo(() => assetFileStore.GetFileSizeAsync(A<DomainId>._, A<DomainId>._, A<long>._, null, default))
                .MustNotHaveHappened();
        }

        private void SetupEvent(IEvent? @event)
        {
            var storedEvent =
                new StoredEvent("stream", "0", -1,
                    new EventData("type", new EnvelopeHeaders(), "payload"));

            var storedEvents = new List<StoredEvent>
            {
                storedEvent
            };

            if (@event != null)
            {
                A.CallTo(() => eventDataFormatter.ParseIfKnown(storedEvent))
                    .Returns(Envelope.Create(@event));
            }
            else
            {
                A.CallTo(() => eventDataFormatter.ParseIfKnown(storedEvent))
                    .Returns(null);
            }

            A.CallTo(() => eventStore.QueryAllAsync("^asset\\-", null, int.MaxValue, default))
                .Returns(storedEvents.ToAsyncEnumerable());
        }
    }
}
