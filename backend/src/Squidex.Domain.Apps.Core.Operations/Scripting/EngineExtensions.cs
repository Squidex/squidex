// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Jint;

namespace Squidex.Domain.Apps.Core.Scripting;

public static class EngineExtensions
{
    public static ScriptExecutionContext GetContext(this Engine engine)
    {
        return ScriptExecutionContext.GetContext(engine);
    }

    public static void Schedule(this Engine engine, Func<CancellationToken, Task> action)
    {
        ScriptExecutionContext.GetContext(engine).Schedule(action);
    }

    public static void Schedule<TResult>(this Engine engine, Func<CancellationToken, Task<TResult>> action, Action<TResult>? callback)
    {
        ScriptExecutionContext.GetContext(engine).Schedule(action, callback);
    }
}
