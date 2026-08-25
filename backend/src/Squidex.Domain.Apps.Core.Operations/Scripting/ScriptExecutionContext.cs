// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Runtime.CompilerServices;
using Acornima.Ast;
using Jint;
using Jint.Native;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Tasks;

namespace Squidex.Domain.Apps.Core.Scripting;

public abstract class ScriptExecutionContext : ScriptVars
{
    private static readonly ConditionalWeakTable<Engine, ScriptExecutionContext> Contexts = new ConditionalWeakTable<Engine, ScriptExecutionContext>();

    public Engine Engine { get; }

    protected ScriptExecutionContext(Engine engine)
    {
        Engine = engine;

        // The extensions only get the engine and resolve the context from there.
        Contexts.AddOrUpdate(engine, this);
    }

    public static ScriptExecutionContext GetContext(Engine engine)
    {
        if (!Contexts.TryGetValue(engine, out var context))
        {
            ThrowHelper.InvalidOperationException("Engine is not attached to a script context.");
            return default!;
        }

        return context;
    }

    public abstract JsValue Evaluate(Prepared<Script> script);

    public abstract Task<JsValue> EvaluateAsync(Prepared<Script> script);

    public abstract void Schedule(Func<CancellationToken, Task> action);

    public abstract void Schedule<TResult>(Func<CancellationToken, Task<TResult>> action, Action<TResult>? callback);
}

public sealed class ScriptExecutionContext<T> : ScriptExecutionContext
{
    private readonly TaskCompletionSource<CompletedValue?> tcs = new TaskCompletionSource<CompletedValue?>(TaskCreationOptions.RunContinuationsAsynchronously);
    private readonly SemaphoreSlim engineLock = new SemaphoreSlim(1);
    private readonly CancellationTokenRegistration cancellationRegistration;
    private readonly CancellationToken cancellationToken;
    private int pendingTasks = 1;

    private sealed class CompletedValue
    {
        public T Value { get; init; }
    }

    private readonly struct Releaser(SemaphoreSlim semaphore) : IDisposable
    {
        public void Dispose()
        {
            semaphore.Release();
        }
    }

    public bool IsCompleted
    {
        get => tcs.Task.IsCompleted;
    }

    internal ScriptExecutionContext(Engine engine, CancellationToken cancellationToken)
        : base(engine)
    {
        this.cancellationToken = cancellationToken;

        // Settle the source on cancellation, so that pending callbacks do not enter the engine anymore.
        cancellationRegistration = cancellationToken.Register(static state =>
        {
            var self = (ScriptExecutionContext<T>)state!;

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

            // The fallback converts javascript values and therefore needs exclusive access to the engine.
            using (await LockEngineAsync())
            {
                return fallback();
            }
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

    public override JsValue Evaluate(Prepared<Script> script)
    {
        // The synchronous path cannot schedule tasks, therefore nothing else can enter the engine.
        return Engine.Evaluate(script);
    }

    public override Task<JsValue> EvaluateAsync(Prepared<Script> script)
    {
        // The lock cannot be taken here, otherwise we would deadlock.
        return Engine.EvaluateAsync(script, cancellationToken);
    }

    public override void Schedule(Func<CancellationToken, Task> action)
    {
        ScheduleCoreAsync(CallWithDummyResult(action), null);
    }

    public override void Schedule<TResult>(Func<CancellationToken, Task<TResult>> action, Action<TResult>? callback)
    {
        ScheduleCoreAsync(action, callback);
    }

    private static Func<CancellationToken, Task<bool>> CallWithDummyResult(Func<CancellationToken, Task> action)
    {
        return async ct =>
        {
            await action(ct);
            return true;
        };
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
                using (await LockEngineAsync())
                {
                    if (!IsCompleted)
                    {
                        // The task can take a while, therefore the callback gets a fresh timeout.
                        Engine.Constraints.Reset();

                        callback?.Invoke(result);
                    }
                }

                TryComplete();
            }
            catch (Exception ex)
            {
                TryFail(ex);
            }
        }

        ScheduleAsync().Forget();
    }

    private async Task<Releaser> LockEngineAsync()
    {
        await engineLock.WaitAsync(cancellationToken);

        return new Releaser(engineLock);
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
