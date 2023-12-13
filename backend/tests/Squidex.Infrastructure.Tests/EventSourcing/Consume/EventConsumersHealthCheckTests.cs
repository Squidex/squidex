// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Squidex.Infrastructure.EventSourcing.Consume;

public class EventConsumersHealthCheckTests
{
    private readonly IEventConsumerManager eventConsumerManager = A.Fake<IEventConsumerManager>();
    private readonly List<EventConsumerInfo> consumers = [];
    private readonly CancellationTokenSource cts = new CancellationTokenSource();
    private readonly CancellationToken ct;
    private readonly EventConsumersHealthCheck sut;

    public EventConsumersHealthCheckTests()
    {
        ct = cts.Token;

        A.CallTo(() => eventConsumerManager.GetConsumersAsync(ct))
            .Returns(consumers);

        sut = new EventConsumersHealthCheck(eventConsumerManager);
    }

    [Fact]
    public async Task Should_return_healthy_if_no_consumer_found()
    {
        var status = await sut.CheckHealthAsync(null!, ct);

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

        var status = await sut.CheckHealthAsync(null!, ct);

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

        var status = await sut.CheckHealthAsync(null!, ct);

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

        var status = await sut.CheckHealthAsync(null!, ct);

        Assert.Equal(HealthStatus.Degraded, status.Status);
    }
}
