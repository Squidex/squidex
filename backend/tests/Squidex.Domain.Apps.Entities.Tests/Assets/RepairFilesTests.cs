// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Assets;
using Squidex.Domain.Apps.Entities.TestHelpers;
using Squidex.Domain.Apps.Events.Assets;
using Squidex.Infrastructure;
using Squidex.Infrastructure.EventSourcing;

namespace Squidex.Domain.Apps.Entities.Assets;

public class RepairFilesTests : GivenContext
{
    private readonly IEventStore eventStore = A.Fake<IEventStore>();
    private readonly IEventFormatter eventFormatter = A.Fake<IEventFormatter>();
    private readonly IAssetFileStore assetFileStore = A.Fake<IAssetFileStore>();
    private readonly RebuildFiles sut;

    public RepairFilesTests()
    {
        sut = new RebuildFiles(assetFileStore, eventFormatter, eventStore);
    }

    [Fact]
    public async Task Should_repair_created_asset_if_not_found()
    {
        var @event = new AssetCreated { AppId = AppId, AssetId = DomainId.NewGuid() };

        SetupEvent(@event);

        A.CallTo(() => assetFileStore.GetFileSizeAsync(AppId.Id, @event.AssetId, 0, null, CancellationToken))
            .Throws(new AssetNotFoundException("file"));

        await sut.RepairAsync(CancellationToken);

        A.CallTo(() => assetFileStore.UploadAsync(AppId.Id, @event.AssetId, 0, null, A<Stream>._, true, CancellationToken))
            .MustHaveHappened();
    }

    [Fact]
    public async Task Should_not_repair_created_asset_if_found()
    {
        var @event = new AssetCreated { AppId = AppId, AssetId = DomainId.NewGuid() };

        SetupEvent(@event);

        A.CallTo(() => assetFileStore.GetFileSizeAsync(AppId.Id, @event.AssetId, 0, null, CancellationToken))
            .Returns(100);

        await sut.RepairAsync(CancellationToken);

        A.CallTo(() => assetFileStore.UploadAsync(AppId.Id, @event.AssetId, 0, null, A<Stream>._, true, A<CancellationToken>._))
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task Should_repair_updated_asset_if_not_found()
    {
        var @event = new AssetUpdated { AppId = AppId, AssetId = DomainId.NewGuid(), FileVersion = 3 };

        SetupEvent(@event);

        A.CallTo(() => assetFileStore.GetFileSizeAsync(AppId.Id, @event.AssetId, 3, null, CancellationToken))
            .Throws(new AssetNotFoundException("file"));

        await sut.RepairAsync(CancellationToken);

        A.CallTo(() => assetFileStore.UploadAsync(AppId.Id, @event.AssetId, 3, null, A<Stream>._, true, CancellationToken))
            .MustHaveHappened();
    }

    [Fact]
    public async Task Should_not_repair_updated_asset_if_found()
    {
        var @event = new AssetUpdated { AppId = AppId, AssetId = DomainId.NewGuid(), FileVersion = 3 };

        SetupEvent(@event);

        A.CallTo(() => assetFileStore.GetFileSizeAsync(AppId.Id, @event.AssetId, 3, null, CancellationToken))
            .Returns(100);

        await sut.RepairAsync(CancellationToken);

        A.CallTo(() => assetFileStore.UploadAsync(AppId.Id, @event.AssetId, 3, null, A<Stream>._, true, A<CancellationToken>._))
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task Should_ignore_old_events()
    {
        SetupEvent(null);

        await sut.RepairAsync(CancellationToken);

        A.CallTo(() => assetFileStore.GetFileSizeAsync(A<DomainId>._, A<DomainId>._, A<long>._, null, A<CancellationToken>._))
            .MustNotHaveHappened();
    }

    private void SetupEvent(IEvent? @event)
    {
        var storedEvent =
            new StoredEvent("stream", "0", -1,
                new EventData("type", [], "payload"));

        var storedEvents = new List<StoredEvent>
        {
            storedEvent
        };

        if (@event != null)
        {
            A.CallTo(() => eventFormatter.ParseIfKnown(storedEvent))
                .Returns(Envelope.Create(@event));
        }
        else
        {
            A.CallTo(() => eventFormatter.ParseIfKnown(storedEvent))
                .Returns(null);
        }

        var streamFilter = StreamFilter.Prefix("asset-");

        A.CallTo(() => eventStore.QueryAllAsync(streamFilter, null, int.MaxValue, CancellationToken))
            .Returns(storedEvents.ToAsyncEnumerable());
    }
}
