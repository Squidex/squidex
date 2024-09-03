// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Acornima.Ast;
using Jint;
using Jint.Native;
using Squidex.Infrastructure.Tasks;

namespace Squidex.Domain.Apps.Core.Scripting;

public abstract class ScriptExecutionContext : ScriptVars
{
    public Engine Engine { get; }

    protected ScriptExecutionContext(Engine engine)
    {
        Engine = engine;
    }

    public abstract JsValue Evaluate(Prepared<Script> script);

    public abstract void Schedule(Func<IScheduler, CancellationToken, Task> action);
}

public sealed class ScriptExecutionContext<T> : ScriptExecutionContext, IScheduler
{
    private readonly TaskCompletionSource<CompletedValue?> tcs = new TaskCompletionSource<CompletedValue?>();
    private readonly CancellationToken cancellationToken;
    private int pendingTasks = 1;

    private sealed class CompletedValue
    {
        public T Value { get; init; }
    }

    public bool IsCompleted
    {
        get => tcs.Task.Status is TaskStatus.RanToCompletion or TaskStatus.Faulted;
    }

    internal ScriptExecutionContext(Engine engine, CancellationToken cancellationToken)
        : base(engine)
    {
        this.cancellationToken = cancellationToken;
    }

    public async Task<T> WaitForCompletionAsync(Func<T> fallback)
    {
        TryComplete();

        var result = await tcs.Task.WithCancellation(cancellationToken);

        if (result == null)
        {
            return fallback();
        }

        return result.Value;
    }

    public void Complete(T value)
    {
        tcs.TrySetResult(new CompletedValue { Value = value });
    }

    public override JsValue Evaluate(Prepared<Script> script)
    {
        lock (Engine)
        {
            return Engine.Evaluate(script);
        }
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

                TryComplete();
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
                Engine.Constraints.Reset();
                action();
            }

            TryComplete();
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
                Engine.Constraints.Reset();
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

    private void TryComplete(CompletedValue? result = null)
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
