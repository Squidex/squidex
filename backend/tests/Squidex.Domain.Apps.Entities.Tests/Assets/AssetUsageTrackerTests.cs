// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using NodaTime;
using Squidex.Domain.Apps.Core.Tags;
using Squidex.Domain.Apps.Entities.Billing;
using Squidex.Domain.Apps.Events.Assets;
using Squidex.Infrastructure;
using Squidex.Infrastructure.EventSourcing;
using Squidex.Infrastructure.States;

namespace Squidex.Domain.Apps.Entities.Assets;

public class AssetUsageTrackerTests
{
    private readonly IAssetLoader assetLoader = A.Fake<IAssetLoader>();
    private readonly ISnapshotStore<AssetUsageTracker.State> store = A.Fake<ISnapshotStore<AssetUsageTracker.State>>();
    private readonly ITagService tagService = A.Fake<ITagService>();
    private readonly IUsageGate usageGate = A.Fake<IUsageGate>();
    private readonly NamedId<DomainId> appId = NamedId.Of(DomainId.NewGuid(), "my-app");
    private readonly DomainId assetId = DomainId.NewGuid();
    private readonly DomainId assetKey;
    private readonly AssetUsageTracker sut;

    public AssetUsageTrackerTests()
    {
        assetKey = DomainId.Combine(appId, assetId);

        sut = new AssetUsageTracker(usageGate, assetLoader, tagService, store);
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

        Assert.Equal(nameof(AssetUsageTracker), consumer.Name);
    }

    public static IEnumerable<object[]> EventData()
    {
        yield return new object[]
        {
            new AssetCreated { FileSize = 128 }, 128, 1
        };

        yield return new object[]
        {
            new AssetUpdated { FileSize = 512 }, 512, 0
        };

        yield return new object[]
        {
            new AssetDeleted { DeletedSize = 512 }, -512, -1
        };
    }

    [Theory]
    [MemberData(nameof(EventData))]
    public async Task Should_increase_usage_if_for_event(AssetEvent @event, long sizeDiff, long countDiff)
    {
        var date = DateTime.UtcNow.Date.AddDays(13);

        @event.AppId = appId;

        var envelope =
            Envelope.Create<IEvent>(@event)
                .SetTimestamp(Instant.FromDateTimeUtc(date));

        await sut.On(new[] { envelope });

        A.CallTo(() => usageGate.TrackAssetAsync(appId.Id, date, sizeDiff, countDiff, default))
            .MustHaveHappened();
    }

    [Fact]
    public async Task Should_write_tags_when_asset_created()
    {
        var @event = new AssetCreated
        {
            AppId = appId,
            Tags = new HashSet<string>
            {
                "tag1",
                "tag2"
            },
            AssetId = assetId
        };

        var envelope =
            Envelope.Create<IEvent>(@event)
                .SetAggregateId(assetKey);

        Dictionary<string, int>? update = null;

        A.CallTo(() => tagService.UpdateAsync(appId.Id, TagGroups.Assets, A<Dictionary<string, int>>._, default))
            .Invokes(x => { update = x.GetArgument<Dictionary<string, int>>(2); });

        await sut.On(new[] { envelope });

        update.Should().BeEquivalentTo(new Dictionary<string, int>
        {
            ["tag1"] = 1,
            ["tag2"] = 1
        });
    }

    [Fact]
    public async Task Should_group_tags_by_app()
    {
        var @event1 = new AssetCreated
        {
            AppId = appId,
            Tags = new HashSet<string>
            {
                "tag1",
                "tag2"
            },
            AssetId = assetId
        };

        var @event2 = new AssetCreated
        {
            AppId = appId,
            Tags = new HashSet<string>
            {
                "tag2",
                "tag3"
            },
            AssetId = assetId
        };

        var envelope1 =
            Envelope.Create<IEvent>(@event1)
                .SetAggregateId(assetKey);

        var envelope2 =
            Envelope.Create<IEvent>(@event2)
                .SetAggregateId(assetKey);

        Dictionary<string, int>? update = null;

        A.CallTo(() => tagService.UpdateAsync(appId.Id, TagGroups.Assets, A<Dictionary<string, int>>._, default))
            .Invokes(x => { update = x.GetArgument<Dictionary<string, int>>(2); });

        await sut.On(new[] { envelope1, envelope2 });

        update.Should().BeEquivalentTo(new Dictionary<string, int>
        {
            ["tag1"] = 1,
            ["tag2"] = 2,
            ["tag3"] = 1
        });

        A.CallTo(() => store.WriteManyAsync(A<IEnumerable<SnapshotWriteJob<AssetUsageTracker.State>>>._, default))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task Should_merge_tags_with_previous_event_on_annotate()
    {
        var @event1 = new AssetCreated
        {
            AppId = appId,
            Tags = new HashSet<string>
            {
                "tag1",
                "tag2"
            },
            AssetId = assetId
        };

        var @event2 = new AssetAnnotated
        {
            AppId = appId,
            Tags = new HashSet<string>
            {
                "tag2",
                "tag3"
            },
            AssetId = assetId
        };

        var envelope1 =
            Envelope.Create<IEvent>(@event1)
                .SetAggregateId(assetKey);

        var envelope2 =
            Envelope.Create<IEvent>(@event2)
                .SetAggregateId(assetKey);

        Dictionary<string, int>? update = null;

        A.CallTo(() => tagService.UpdateAsync(appId.Id, TagGroups.Assets, A<Dictionary<string, int>>._, default))
            .Invokes(x => { update = x.GetArgument<Dictionary<string, int>>(2); });

        await sut.On(new[] { envelope1, envelope2 });

        update.Should().BeEquivalentTo(new Dictionary<string, int>
        {
            ["tag1"] = 0,
            ["tag2"] = 1,
            ["tag3"] = 1
        });
    }

    [Fact]
    public async Task Should_merge_tags_with_previous_event_on_annotate_from_other_batch()
    {
        var @event1 = new AssetCreated
        {
            AppId = appId,
            Tags = new HashSet<string>
            {
                "tag1",
                "tag2"
            },
            AssetId = assetId
        };

        var @event2 = new AssetAnnotated
        {
            AppId = appId,
            Tags = new HashSet<string>
            {
                "tag2",
                "tag3"
            },
            AssetId = assetId
        };

        var envelope1 =
            Envelope.Create<IEvent>(@event1)
                .SetAggregateId(assetKey);

        var envelope2 =
            Envelope.Create<IEvent>(@event2)
                .SetAggregateId(assetKey);

        Dictionary<string, int>? update = null;

        A.CallTo(() => tagService.UpdateAsync(appId.Id, TagGroups.Assets, A<Dictionary<string, int>>._, default))
            .Invokes(x => { update = x.GetArgument<Dictionary<string, int>>(2); });

        await sut.On(new[] { envelope1 });
        await sut.On(new[] { envelope2 });

        update.Should().BeEquivalentTo(new Dictionary<string, int>
        {
            ["tag1"] = -1,
            ["tag2"] = 0,
            ["tag3"] = 1
        });
    }

    [Fact]
    public async Task Should_merge_tags_with_previous_event_on_delete()
    {
        var @event1 = new AssetCreated
        {
            AppId = appId,
            Tags = new HashSet<string>
            {
                "tag1",
                "tag2"
            },
            AssetId = assetId
        };

        var @event2 = new AssetDeleted { AppId = appId, AssetId = assetId };

        var envelope1 =
            Envelope.Create<IEvent>(@event1)
                .SetAggregateId(assetKey);

        var envelope2 =
            Envelope.Create<IEvent>(@event2)
                .SetAggregateId(assetKey);

        Dictionary<string, int>? update = null;

        A.CallTo(() => tagService.UpdateAsync(appId.Id, TagGroups.Assets, A<Dictionary<string, int>>._, default))
            .Invokes(x => { update = x.GetArgument<Dictionary<string, int>>(2); });

        await sut.On(new[] { Envelope.Create<IEvent>(@event1), Envelope.Create<IEvent>(@event2) });

        update.Should().BeEquivalentTo(new Dictionary<string, int>
        {
            ["tag1"] = 0,
            ["tag2"] = 0
        });
    }

    [Fact]
    public async Task Should_merge_tags_with_stored_state_if_previous_event_not_in_cached()
    {
        var state = new AssetUsageTracker.State
        {
            Tags = new HashSet<string>
            {
                "tag1",
                "tag2"
            }
        };

        A.CallTo(() => store.ReadAsync(assetKey, default))
            .Returns(new SnapshotResult<AssetUsageTracker.State>(assetKey, state, 0));

        var @event = new AssetDeleted { AppId = appId, AssetId = assetId };

        var envelope =
            Envelope.Create<IEvent>(@event)
                .SetAggregateId(assetKey);

        Dictionary<string, int>? update = null;

        A.CallTo(() => tagService.UpdateAsync(appId.Id, TagGroups.Assets, A<Dictionary<string, int>>._, default))
            .Invokes(x => { update = x.GetArgument<Dictionary<string, int>>(2); });

        await sut.On(new[] { envelope });

        update.Should().BeEquivalentTo(new Dictionary<string, int>
        {
            ["tag1"] = -1,
            ["tag2"] = -1
        });
    }

    [Fact]
    public async Task Should_merge_tags_with_asset_if_previous_tags_not_in_store()
    {
        IAssetEntity asset = new AssetEntity
        {
            Tags = new HashSet<string>
            {
                "tag1",
                "tag2"
            }
        };

        A.CallTo(() => assetLoader.GetAsync(appId.Id, assetId, 41, default))
            .Returns(asset);

        var @event = new AssetDeleted { AppId = appId, AssetId = assetId };

        var envelope =
            Envelope.Create<IEvent>(@event)
                .SetEventStreamNumber(42)
                .SetAggregateId(assetKey);

        Dictionary<string, int>? update = null;

        A.CallTo(() => tagService.UpdateAsync(appId.Id, TagGroups.Assets, A<Dictionary<string, int>>._, default))
            .Invokes(x => { update = x.GetArgument<Dictionary<string, int>>(2); });

        await sut.On(new[] { envelope });

        update.Should().BeEquivalentTo(new Dictionary<string, int>
        {
            ["tag1"] = -1,
            ["tag2"] = -1
        });
    }
}
