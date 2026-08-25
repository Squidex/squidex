// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Runtime.CompilerServices;
using Jint;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Core.Scripting;

public class ScriptExecutionContext : ScriptVars
{
    private static readonly ConditionalWeakTable<Engine, ScriptExecutionContext> Contexts = [];

    internal ScriptExecutionContext(Engine engine)
    {
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

    public virtual void Fail(Exception exception)
    {
        // The synchronous path reports the error over the exception of the evaluation itself.
    }

    public virtual void Schedule(Func<CancellationToken, Task> action)
    {
        ThrowHelper.NotSupportedException("Async operations are not allowed for this script.");
    }

    public virtual void Schedule<TResult>(Func<CancellationToken, Task<TResult>> action, Action<TResult>? callback)
    {
        ThrowHelper.NotSupportedException("Async operations are not allowed for this script.");
    }
}
