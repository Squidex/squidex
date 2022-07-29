// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using FakeItEasy;
using FluentAssertions;
using NodaTime;
using Squidex.Domain.Apps.Core.Tags;
using Squidex.Domain.Apps.Events.Assets;
using Squidex.Infrastructure;
using Squidex.Infrastructure.EventSourcing;
using Squidex.Infrastructure.States;
using Squidex.Infrastructure.UsageTracking;
using Xunit;

namespace Squidex.Domain.Apps.Entities.Assets
{
    public class AssetUsageTrackerTests
    {
        private readonly IAssetLoader assetLoader = A.Fake<IAssetLoader>();
        private readonly ISnapshotStore<AssetUsageTracker.State> store = A.Fake<ISnapshotStore<AssetUsageTracker.State>>();
        private readonly ITagService tagService = A.Fake<ITagService>();
        private readonly IUsageTracker usageTracker = A.Fake<IUsageTracker>();
        private readonly NamedId<DomainId> appId = NamedId.Of(DomainId.NewGuid(), "my-app");
        private readonly DomainId assetId = DomainId.NewGuid();
        private readonly DomainId assetKey;
        private readonly AssetUsageTracker sut;

        public AssetUsageTrackerTests()
        {
            assetKey = DomainId.Combine(appId, assetId);

            A.CallTo(() => usageTracker.FallbackCategory)
                .Returns("*");

            sut = new AssetUsageTracker(usageTracker, assetLoader, tagService, store);
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

        [Fact]
        public async Task Should_get_total_size_from_summary_date()
        {
            A.CallTo(() => usageTracker.GetAsync($"{appId.Id}_Assets", default, default, null, default))
                .Returns(new Counters { ["TotalSize"] = 2048 });

            var size = await sut.GetTotalSizeAsync(appId.Id);

            Assert.Equal(2048, size);
        }

        [Theory]
        [InlineData("*")]
        [InlineData("Default")]
        public async Task Should_get_counters_from_categories(string category)
        {
            var dateFrom = new DateTime(2018, 01, 05);
            var dateTo = dateFrom.AddDays(3);

            A.CallTo(() => usageTracker.QueryAsync($"{appId.Id}_Assets", dateFrom, dateTo, default))
                .Returns(new Dictionary<string, List<(DateTime, Counters)>>
                {
                    [category] = new List<(DateTime, Counters)>
                    {
                        (dateFrom.AddDays(0), new Counters
                        {
                            ["TotalSize"] = 128,
                            ["TotalAssets"] = 2
                        }),
                        (dateFrom.AddDays(1), new Counters
                        {
                            ["TotalSize"] = 256,
                            ["TotalAssets"] = 3
                        }),
                        (dateFrom.AddDays(2), new Counters
                        {
                            ["TotalSize"] = 512,
                            ["TotalAssets"] = 4
                        })
                    }
                });

            var result = await sut.QueryAsync(appId.Id, dateFrom, dateTo);

            result.Should().BeEquivalentTo(new List<AssetStats>
            {
                new AssetStats(dateFrom.AddDays(0), 2, 128),
                new AssetStats(dateFrom.AddDays(1), 3, 256),
                new AssetStats(dateFrom.AddDays(2), 4, 512)
            });
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
        public async Task Should_increase_usage_if_asset_created(AssetEvent @event, long sizeDiff, long countDiff)
        {
            var date = DateTime.UtcNow.Date.AddDays(13);

            @event.AppId = appId;

            var envelope =
                Envelope.Create<IEvent>(@event)
                    .SetTimestamp(Instant.FromDateTimeUtc(date));

            Counters? countersSummary = null;
            Counters? countersDate = null;

            A.CallTo(() => usageTracker.TrackAsync(default, $"{appId.Id}_Assets", null, A<Counters>._, default))
                .Invokes(x => countersSummary = x.GetArgument<Counters>(3));

            A.CallTo(() => usageTracker.TrackAsync(date, $"{appId.Id}_Assets", null, A<Counters>._, default))
                .Invokes(x => countersDate = x.GetArgument<Counters>(3));

            await sut.On(new[] { envelope });

            var expected = new Counters
            {
                ["TotalSize"] = sizeDiff,
                ["TotalAssets"] = countDiff
            };

            countersSummary.Should().BeEquivalentTo(expected);
            countersDate.Should().BeEquivalentTo(expected);
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
}
