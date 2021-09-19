// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FakeItEasy;
using FluentAssertions;
using NodaTime;
using Squidex.Domain.Apps.Events.Assets;
using Squidex.Infrastructure;
using Squidex.Infrastructure.EventSourcing;
using Squidex.Infrastructure.UsageTracking;
using Xunit;

namespace Squidex.Domain.Apps.Entities.Assets
{
    public class AssetUsageTrackerTests
    {
        private readonly IUsageTracker usageTracker = A.Fake<IUsageTracker>();
        private readonly NamedId<DomainId> appId = NamedId.Of(DomainId.NewGuid(), "my-app");
        private readonly AssetUsageTracker sut;

        public AssetUsageTrackerTests()
        {
            sut = new AssetUsageTracker(usageTracker);
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
                Envelope.Create(@event)
                    .SetTimestamp(Instant.FromDateTimeUtc(date));

            Counters? countersSummary = null;
            Counters? countersDate = null;

            A.CallTo(() => usageTracker.TrackAsync(default, $"{appId.Id}_Assets", null, A<Counters>._, default))
                .Invokes(x => countersSummary = x.GetArgument<Counters>(3));

            A.CallTo(() => usageTracker.TrackAsync(date, $"{appId.Id}_Assets", null, A<Counters>._, default))
                .Invokes(x => countersDate = x.GetArgument<Counters>(3));

            await sut.On(envelope);

            var expected = new Counters
            {
                ["TotalSize"] = sizeDiff,
                ["TotalAssets"] = countDiff
            };

            countersSummary.Should().BeEquivalentTo(expected);
            countersDate.Should().BeEquivalentTo(expected);
        }
    }
}
