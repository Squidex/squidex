// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Jint;
using Squidex.Infrastructure.Tasks;
using Squidex.Text;

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

#pragma warning disable MA0048 // File name must match type name
public interface IScheduler
#pragma warning restore MA0048 // File name must match type name
{
    void Run(Action? action);

    void Run<T>(Action<T>? action, T argument);
}

public sealed class ScriptExecutionContext<T> : ScriptExecutionContext, IScheduler
{
    private readonly TaskCompletionSource<T?> tcs = new TaskCompletionSource<T?>();
    private readonly CancellationToken cancellationToken;
    private int pendingTasks;

    public bool IsCompleted
    {
        get => tcs.Task.Status is TaskStatus.RanToCompletion or TaskStatus.Faulted;
    }

    internal ScriptExecutionContext(Engine engine, CancellationToken cancellationToken)
        : base(engine)
    {
        this.cancellationToken = cancellationToken;
    }

    public Task<T?> CompleteAsync()
    {
        if (pendingTasks <= 0)
        {
            tcs.TrySetResult(default);
        }

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
            try
            {
                Interlocked.Increment(ref pendingTasks);

                await action(this, cancellationToken);

                if (Interlocked.Decrement(ref pendingTasks) <= 0)
                {
                    tcs.TrySetResult(default);
                }
            }
            catch (Exception ex)
            {
                tcs.TrySetException(ex);
            }
        }

        ScheduleAsync().Forget();
    }

    public ScriptExecutionContext<T> ExtendAsync(IEnumerable<IJintExtension> extensions)
    {
        foreach (var extension in extensions)
        {
            extension.ExtendAsync(this);
        }

        return this;
    }

    public ScriptExecutionContext<T> Extend(IEnumerable<IJintExtension> extensions)
    {
        foreach (var extension in extensions)
        {
            extension.Extend(this);
        }

        return this;
    }

    public ScriptExecutionContext<T> Extend(ScriptVars vars, ScriptOptions options)
    {
        var engine = Engine;

        if (options.AsContext)
        {
            var contextInstance = new WritableContext(engine, vars);

            foreach (var (key, value) in vars.Where(x => x.Value != null))
            {
                this[key.ToCamelCase()] = value;
            }

            engine.SetValue("ctx", contextInstance);
            engine.SetValue("context", contextInstance);
        }
        else
        {
            foreach (var (key, value) in vars)
            {
                var property = key.ToCamelCase();

                if (value != null)
                {
                    engine.SetValue(property, value);

                    this[property] = value;
                }
            }
        }

        engine.SetValue("async", true);

        return this;
    }

    void IScheduler.Run(Action? action)
    {
        if (IsCompleted || action == null)
        {
            return;
        }

        Engine.ResetConstraints();
        action();
    }

    void IScheduler.Run<TArg>(Action<TArg>? action, TArg argument)
    {
        if (IsCompleted || action == null)
        {
            return;
        }

        Engine.ResetConstraints();
        action(argument);
    }
}
