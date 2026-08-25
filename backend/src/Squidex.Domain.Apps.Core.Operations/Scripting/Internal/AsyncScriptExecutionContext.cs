// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Jint;
using Squidex.Infrastructure.Tasks;

namespace Squidex.Domain.Apps.Core.Scripting.Internal;

internal sealed class AsyncScriptExecutionContext<T> : ScriptExecutionContext
{
    private readonly TaskCompletionSource<CompletedValue?> tcs = new TaskCompletionSource<CompletedValue?>(TaskCreationOptions.RunContinuationsAsynchronously);
    private readonly CancellationTokenRegistration cancellationRegistration;
    private readonly CancellationToken cancellationToken;
    private readonly JintScript script;
    private readonly Engine engine;
    private int pendingTasks = 1;

    private sealed class CompletedValue
    {
        public T Value { get; init; }
    }

    public bool IsCompleted
    {
        get => tcs.Task.IsCompleted;
    }

    internal AsyncScriptExecutionContext(Engine engine, JintScript script, CancellationToken ct)
        : base(engine)
    {
        this.engine = engine;

        // The lock belongs to the script, because the engine is shared between all executions.
        this.script = script;

        cancellationToken = ct;

        // Settle the source on cancellation, so that pending callbacks do not enter the engine anymore.
        cancellationRegistration = cancellationToken.Register(static state =>
        {
            var self = (AsyncScriptExecutionContext<T>)state!;

            self.tcs.TrySetCanceled(self.cancellationToken);
        },
        this);
    }

    public async Task<T> WaitForCompletionAsync(Func<T> fallback)
    {
        TryComplete();
        try
        {
            var result = await tcs.Task;
            if (result != null)
            {
                return result.Value;
            }

            return await script.RunLockedAsync(() => fallback(), cancellationToken);
        }
        finally
        {
            await cancellationRegistration.DisposeAsync();
        }
    }

    public void Complete(T value)
    {
        tcs.TrySetResult(new CompletedValue { Value = value });
    }

    public override void Fail(Exception exception)
    {
        TryFail(exception);
    }

    public override void Schedule(Func<CancellationToken, Task> action)
    {
        ScheduleCoreAsync(async ct =>
        {
            await action(ct);
            return true;
        },
        null);
    }

    public override void Schedule<TResult>(Func<CancellationToken, Task<TResult>> action, Action<TResult>? callback)
    {
        ScheduleCoreAsync(async ct =>
        {
            var result = await action(ct);
            return result;
        },
        callback);
    }

    private void ScheduleCoreAsync<TResult>(Func<CancellationToken, Task<TResult>> action, Action<TResult>? callback)
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
                // The action must not touch the engine, so that parallel tasks do not block each other.
                var result = await action(cancellationToken);

                // The callback converts javascript values and is therefore the only part that needs the lock.
                await RunLockedAsync(() => callback?.Invoke(result));

                TryComplete();
            }
            catch (Exception ex)
            {
                TryFail(ex);
            }
        }

        ScheduleAsync().Forget();
    }

    private Task<bool> RunLockedAsync(Action action)
    {
        // The lock is owned by the script, because the engine is shared between all executions.
        return script.RunLockedAsync(() =>
        {
            // Late callbacks must not touch the engine anymore, the next execution might have started.
            if (IsCompleted)
            {
                return true;
            }

            // The task can take a while, therefore the action gets a fresh timeout.
            engine.Constraints.Reset();

            action();

            // The evaluation does not wait for the promises anymore, therefore the continuations that
            // the callback has unblocked have to be executed here, while the lock is still held.
            engine.Advanced.ProcessTasks();
            return true;
        },
        cancellationToken);
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
