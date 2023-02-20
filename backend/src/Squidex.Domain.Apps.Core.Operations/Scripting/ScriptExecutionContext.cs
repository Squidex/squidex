// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Jint;
using Squidex.Infrastructure.Tasks;

namespace Squidex.Domain.Apps.Core.Scripting;

public abstract class ScriptExecutionContext : ScriptContext
{
    public Engine Engine { get; }

    protected ScriptExecutionContext(Engine engine)
    {
        Engine = engine;
    }

    public abstract void Schedule(Func<IScheduler, CancellationToken, Task> action);
}

public sealed class ScriptExecutionContext<T> : ScriptExecutionContext, IScheduler
{
    private readonly TaskCompletionSource<T> tcs = new TaskCompletionSource<T>();
    private readonly CancellationToken cancellationToken;
    private int pendingTasks = 1;

    public bool IsCompleted
    {
        get => tcs.Task.Status is TaskStatus.RanToCompletion or TaskStatus.Faulted;
    }

    internal ScriptExecutionContext(Engine engine, CancellationToken cancellationToken)
        : base(engine)
    {
        this.cancellationToken = cancellationToken;
    }

    public Task<T> CompleteAsync()
    {
        TryComplete(default!);

        return tcs.Task.WithCancellation(cancellationToken);
    }

    public void Complete(T value)
    {
        tcs.TrySetResult(value);
    }

    public override void Schedule(Func<IScheduler, CancellationToken, Task> action)
    {
        if (IsCompleted)
        {
            return;
        }

        async Task ScheduleAsync()
        {
            TryStart();
            try
            {
                await action(this, cancellationToken);

                TryComplete(default!);
            }
            catch (Exception ex)
            {
                TryFail(ex);
            }
        }

        ScheduleAsync().Forget();
    }

    void IScheduler.Run(Action? action)
    {
        if (IsCompleted || action == null)
        {
            return;
        }

        TryStart();
        try
        {
            lock (Engine)
            {
                Engine.ResetConstraints();
                action();
            }

            TryComplete(default!);
        }
        catch (Exception ex)
        {
            TryFail(ex);
        }
    }

    void IScheduler.Run<TArg>(Action<TArg>? action, TArg argument)
    {
        if (IsCompleted || action == null)
        {
            return;
        }

        TryStart();
        try
        {
            lock (Engine)
            {
                Engine.ResetConstraints();
                action(argument);
            }

            TryComplete(default!);
        }
        catch (Exception ex)
        {
            TryFail(ex);
        }
    }

    private void TryFail(Exception exception)
    {
        tcs.TrySetException(exception);
    }

    private void TryStart()
    {
        Interlocked.Increment(ref pendingTasks);
    }

    private void TryComplete(T result)
    {
        if (Interlocked.Decrement(ref pendingTasks) <= 0)
        {
            tcs.TrySetResult(result);
        }
    }
}

#pragma warning disable MA0048 // File name must match type name
public interface IScheduler
#pragma warning restore MA0048 // File name must match type name
{
    void Run(Action? action);

    void Run<T>(Action<T>? action, T argument);
}
