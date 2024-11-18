// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Squidex.Infrastructure.Tasks;

#pragma warning disable MA0048 // File name must match type name
public delegate Task ReentrantSchedulerTask(CancellationToken ct);
#pragma warning restore MA0048 // File name must match type name

public sealed class ReentrantScheduler(int maxDegreeOfParallelism)
{
    private readonly TaskScheduler taskScheduler = new LimitedConcurrencyLevelTaskScheduler(maxDegreeOfParallelism);

    public Task ScheduleAsync(ReentrantSchedulerTask action,
        CancellationToken ct = default)
    {
        var inner = Task<Task>.Factory.StartNew(() => action(ct), ct, TaskCreationOptions.DenyChildAttach, taskScheduler);

        return inner.Unwrap();
    }

    public Task Schedule(Action action)
    {
        return Task<bool>.Factory.StartNew(() =>
        {
            action();

            return false;
        }, default, TaskCreationOptions.DenyChildAttach, taskScheduler);
    }
}
