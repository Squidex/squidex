// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Threading.Tasks;
using FakeItEasy;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Orleans;
using Orleans.Concurrency;
using Squidex.Infrastructure.EventSourcing.Grains;
using Squidex.Infrastructure.Orleans;
using Xunit;

namespace Squidex.Infrastructure.EventSourcing
{
    public class EventConsumersHealthCheckTests
    {
        private readonly IGrainFactory grainFactory = A.Fake<IGrainFactory>();
        private readonly IEventConsumerManagerGrain eventConsumerManager = A.Fake<IEventConsumerManagerGrain>();
        private readonly List<EventConsumerInfo> consumers = new List<EventConsumerInfo>();
        private readonly EventConsumersHealthCheck sut;

        public EventConsumersHealthCheckTests()
        {
            A.CallTo(() => grainFactory.GetGrain<IEventConsumerManagerGrain>(SingleGrain.Id, null))
                .Returns(eventConsumerManager);

            A.CallTo(() => eventConsumerManager.GetConsumersAsync())
                .Returns(consumers.AsImmutable());

            sut = new EventConsumersHealthCheck(grainFactory);
        }

        [Fact]
        public async Task Should_return_healthy_if_no_consumer_found()
        {
            var status = await sut.CheckHealthAsync(null!);

            Assert.Equal(HealthStatus.Healthy, status.Status);
        }

        [Fact]
        public async Task Should_return_healthy_if_no_consumer_failed()
        {
            consumers.Add(new EventConsumerInfo
            {
                Name = "Consumer1"
            });

            consumers.Add(new EventConsumerInfo
            {
                Name = "Consumer2"
            });

            consumers.Add(new EventConsumerInfo
            {
                Name = "Consumer2"
            });

            var status = await sut.CheckHealthAsync(null!);

            Assert.Equal(HealthStatus.Healthy, status.Status);
        }

        [Fact]
        public async Task Should_return_unhealthy_if_all_consumers_failed()
        {
            consumers.Add(new EventConsumerInfo
            {
                Name = "Consumer1",
                Error = "Failed1"
            });

            consumers.Add(new EventConsumerInfo
            {
                Name = "Consumer2",
                Error = "Failed2"
            });

            consumers.Add(new EventConsumerInfo
            {
                Name = "Consumer3",
                Error = "Failed3"
            });

            var status = await sut.CheckHealthAsync(null!);

            Assert.Equal(HealthStatus.Unhealthy, status.Status);
        }

        [Fact]
        public async Task Should_return_degrated_if_at_least_one_consumers_failed()
        {
            consumers.Add(new EventConsumerInfo
            {
                Name = "Consumer1",
                Error = "Failed1"
            });

            consumers.Add(new EventConsumerInfo
            {
                Name = "Consumer2",
                IsStopped = true
            });

            consumers.Add(new EventConsumerInfo
            {
                Name = "Consumer3",
                IsStopped = false
            });

            var status = await sut.CheckHealthAsync(null!);

            Assert.Equal(HealthStatus.Degraded, status.Status);
        }
    }
}
