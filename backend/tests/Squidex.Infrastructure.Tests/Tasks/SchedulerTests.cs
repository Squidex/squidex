// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Concurrent;

#pragma warning disable MA0040 // Forward the CancellationToken parameter to methods that take one

namespace Squidex.Infrastructure.Tasks;

public class SchedulerTests
{
    private readonly ConcurrentBag<int> actuals = [];
    private readonly Scheduler sut = new Scheduler();

    [Fact]
    public async Task Should_schedule_single_task()
    {
        ScheduleAsync(1, sut);

        await sut.CompleteAsync();

        Assert.Equal(new[] { 1 }, actuals.ToArray());
    }

    [Fact]
    public async Task Should_schedule_lot_of_tasks_with_limited_concurrency()
    {
        var limited = new Scheduler(1);

        for (var i = 1; i <= 10; i++)
        {
            ScheduleAsync(i, limited);
        }

        await limited.CompleteAsync();

        Assert.Equal(Enumerable.Range(1, 10).ToArray(), actuals.Order().ToArray());
    }

    [Fact]
    public async Task Should_schedule_multiple_tasks()
    {
        ScheduleAsync(1, sut);
        ScheduleAsync(2, sut);

        await sut.CompleteAsync();

        Assert.Equal(new[] { 1, 2 }, actuals.Order().ToArray());
    }

    [Fact]
    public async Task Should_schedule_multiple_synchronous_tasks()
    {
        Schedule(1, sut);
        Schedule(2, sut);

        await sut.CompleteAsync();

        Assert.Equal(new[] { 1, 2 }, actuals.Order().ToArray());
    }

    [Fact]
    public async Task Should_schedule_nested_tasks()
    {
        sut.Schedule(async _ =>
        {
            await Task.Delay(1);

            actuals.Add(1);

            sut.Schedule(async _ =>
            {
                await Task.Delay(1);

                actuals.Add(2);

                ScheduleAsync(3, sut);
            });
        });

        await sut.CompleteAsync();

        Assert.Equal(new[] { 1, 2, 3 }, actuals.Order().ToArray());
    }

    [Fact]
    public async Task Should_ignore_schedule_after_completion()
    {
        ScheduleAsync(1, sut);

        await sut.CompleteAsync();

        ScheduleAsync(3, sut);

        Assert.Equal(new[] { 1 }, actuals.Order().ToArray());
    }

    private void ScheduleAsync(int value, Scheduler target)
    {
        target.Schedule(async _ =>
        {
            await Task.Delay(1);

            actuals.Add(value);
        });
    }

    private void Schedule(int value, Scheduler target)
    {
        target.Schedule(_ =>
        {
            actuals.Add(value);

            return Task.CompletedTask;
        });
    }
}
