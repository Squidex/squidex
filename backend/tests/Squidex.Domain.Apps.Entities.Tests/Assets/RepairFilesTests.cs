// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.IO;
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
        private readonly RepairFiles sut;

        public RepairFilesTests()
        {
            sut = new RepairFiles(assetFileStore, eventStore, eventDataFormatter);
        }

        [Fact]
        public async Task Should_repair_created_asset_if_not_found()
        {
            var @event = new AssetCreated { AppId = appId, AssetId = DomainId.NewGuid() };

            SetupEvent(@event);

            A.CallTo(() => assetFileStore.GetFileSizeAsync(appId.Id, @event.AssetId, 0, default))
                .Throws(new AssetNotFoundException("file"));

            await sut.RepairAsync();

            A.CallTo(() => assetFileStore.UploadAsync(appId.Id, @event.AssetId, 0, A<Stream>._, default))
                .MustHaveHappened();
        }

        [Fact]
        public async Task Should_not_repair_created_asset_if_found()
        {
            var @event = new AssetCreated { AppId = appId, AssetId = DomainId.NewGuid() };

            SetupEvent(@event);

            A.CallTo(() => assetFileStore.GetFileSizeAsync(appId.Id, @event.AssetId, 0, default))
                .Returns(100);

            await sut.RepairAsync();

            A.CallTo(() => assetFileStore.UploadAsync(appId.Id, @event.AssetId, 0, A<Stream>._, default))
                .MustNotHaveHappened();
        }

        [Fact]
        public async Task Should_repair_updated_asset_if_not_found()
        {
            var @event = new AssetUpdated { AppId = appId, AssetId = DomainId.NewGuid(), FileVersion = 3 };

            SetupEvent(@event);

            A.CallTo(() => assetFileStore.GetFileSizeAsync(appId.Id, @event.AssetId, 3, default))
                .Throws(new AssetNotFoundException("file"));

            await sut.RepairAsync();

            A.CallTo(() => assetFileStore.UploadAsync(appId.Id, @event.AssetId, 3, A<Stream>._, default))
                .MustHaveHappened();
        }

        [Fact]
        public async Task Should_not_repair_updated_asset_if_found()
        {
            var @event = new AssetUpdated { AppId = appId, AssetId = DomainId.NewGuid(), FileVersion = 3 };

            SetupEvent(@event);

            A.CallTo(() => assetFileStore.GetFileSizeAsync(appId.Id, @event.AssetId, 3, default))
                .Returns(100);

            await sut.RepairAsync();

            A.CallTo(() => assetFileStore.UploadAsync(appId.Id, @event.AssetId, 3, A<Stream>._, default))
                .MustNotHaveHappened();
        }

        [Fact]
        public async Task Should_ignore_old_events()
        {
            SetupEvent(null);

            await sut.RepairAsync();

            A.CallTo(() => assetFileStore.GetFileSizeAsync(A<DomainId>._, A<DomainId>._, A<long>._, default))
                .MustNotHaveHappened();
        }

        private void SetupEvent(IEvent? @event)
        {
            var storedEvent = new StoredEvent("stream", "0", -1, new EventData("type", new EnvelopeHeaders(), "payload"));

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

            A.CallTo(() => eventStore.QueryAsync(A<Func<StoredEvent, Task>>._, "^asset\\-", null, default))
                .Invokes(x =>
                {
                    var callback = x.GetArgument<Func<StoredEvent, Task>>(0)!;

                    callback(storedEvent).Wait();
                });
        }
    }
}
