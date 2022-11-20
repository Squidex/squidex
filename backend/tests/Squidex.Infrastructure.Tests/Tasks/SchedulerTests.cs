// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Concurrent;

namespace Squidex.Infrastructure.Tasks;

public class SchedulerTests
{
    private readonly ConcurrentBag<int> actuals = new ConcurrentBag<int>();
    private readonly Scheduler sut = new Scheduler();

    [Fact]
    public async Task Should_schedule_single_task()
    {
        Schedule(1);

        await sut.CompleteAsync();

        Assert.Equal(new[] { 1 }, actuals.ToArray());
    }

    [Fact]
    public async Task Should_schedule_lot_of_tasks_with_limited_concurrency()
    {
        var limited = new Scheduler(1);

        for (var i = 1; i <= 10; i++)
        {
            Schedule(i, limited);
        }

        await limited.CompleteAsync();

        Assert.Equal(Enumerable.Range(1, 10).ToArray(), actuals.OrderBy(x => x).ToArray());
    }

    [Fact]
    public async Task Should_schedule_multiple_tasks()
    {
        Schedule(1);
        Schedule(2);

        await sut.CompleteAsync();

        Assert.Equal(new[] { 1, 2 }, actuals.OrderBy(x => x).ToArray());
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

                Schedule(3);
            });
        });

        await sut.CompleteAsync();

        Assert.Equal(new[] { 1, 2, 3 }, actuals.OrderBy(x => x).ToArray());
    }

    [Fact]
    public async Task Should_ignore_schedule_after_completion()
    {
        Schedule(1);

        await sut.CompleteAsync();

        Schedule(3);

        await Task.Delay(50);

        Assert.Equal(new[] { 1 }, actuals.OrderBy(x => x).ToArray());
    }

    private void Schedule(int value)
    {
        sut.Schedule(async _ =>
        {
            await Task.Delay(1);

            actuals.Add(value);
        });
    }

    private void Schedule(int value, Scheduler target)
    {
        target.Schedule(async _ =>
        {
            await Task.Delay(1);

            actuals.Add(value);
        });
    }
}
