// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Diagnostics;
using Acornima;
using Acornima.Ast;
using Jint;
using Jint.Constraints;
using Jint.Native;
using Jint.Native.Promise;
using Jint.Runtime;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Core.Scripting.ContentWrapper;
using Squidex.Domain.Apps.Core.Scripting.Internal;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Json.Objects;
using Squidex.Infrastructure.Translations;
using Squidex.Infrastructure.Validation;

namespace Squidex.Domain.Apps.Core.Scripting;

internal sealed class JintScript : IScript, IAsyncScript, IDisposable
{
    // The engine is shared between all executions, therefore only one of them can run at a time.
    private readonly SemaphoreSlim executionGate = new SemaphoreSlim(1);

    // The engine is also shared with the callbacks of the scheduled tasks, which run on other threads.
    private readonly SemaphoreSlim engineLock = new SemaphoreSlim(1);

    // Jint only creates the constraint for a token that can actually be cancelled, therefore the engine is
    // built with a placeholder that is never cancelled and repointed for each execution.
    private readonly CancellationTokenSource placeholder = new CancellationTokenSource();
    private readonly CancellationConstraint? cancellation;
    private readonly Engine engine;
    private readonly Prepared<Script> parsed;
    private readonly ScriptOptions scriptOptions;
    private readonly JintScriptOptions engineOptions;
    private readonly GlobalSnapshot snapshot;

    public JintScript(
        Prepared<Script> parsed,
        ScriptOptions scriptOptions,
        JintScriptOptions engineOptions,
        IJintExtension[] extensions,
        bool allowAsync)
    {
        this.parsed = parsed;
        this.scriptOptions = scriptOptions;
        this.engineOptions = engineOptions;

        engine = new Engine(options =>
        {
            options.AddObjectConverter(JintObjectConverter.Instance, JintObjectConverter.HandledTypes);
            options.AddObjectConverter(EnumToStringConverter.Instance);
            options.AllowClrWrite(!scriptOptions.Readonly);
            options.SetTypeConverter(engine => new CustomClrConverter(engine));
            options.SetReferencesResolver(NullPropagation.Instance);
            options.Strict();

            if (!Debugger.IsAttached)
            {
                options.Constraints.PromiseTimeout = engineOptions.TimeoutPromise;
                options.TimeoutInterval(engineOptions.TimeoutScript);
                options.CancellationToken(placeholder.Token);
            }
        });

        cancellation = engine.Constraints.Find<CancellationConstraint>();

        // The extensions resolve the variables from the context of the current execution, therefore they
        // are registered once, even though the variables are different for each execution.
        foreach (var extension in extensions)
        {
            extension.Extend(engine);
        }

        if (allowAsync)
        {
            foreach (var extension in extensions)
            {
                extension.ExtendAsync(engine);
            }
        }

        if (scriptOptions.CanDisallow)
        {
            engine.AddDisallow();
        }

        if (scriptOptions.CanReject)
        {
            engine.AddReject();
        }

        // The evaluation does not wait for the promises anymore, therefore an unhandled rejection would be
        // swallowed. The tracker only fires when nothing handles the rejection.
        engine.Advanced.PromiseRejectionTracker += (_, args) =>
        {
            if (args.Operation == PromiseRejectionOperation.Reject)
            {
                ScriptExecutionContext.GetContext(engine).Fail(
                    new JavaScriptException($"Promise was rejected with value {args.Value}."));
            }
        };

        snapshot = engine.Advanced.CaptureGlobalSnapshot();
    }

    public JsonValue Execute(ScriptVars vars)
    {
        Guard.NotNull(vars);

        return Run(vars, (vars, _) =>
        {
            JsonValue? completed = null;

            engine.SetValue("complete", new Action<JsValue?>(value =>
            {
                completed ??= JsonMapper.Map(value);
            }));

            var result = engine.Evaluate(parsed);

            return completed ?? JsonMapper.Map(result);
        });
    }

    public ValueTask<JsonValue> ExecuteAsync(ScriptVars vars,
        CancellationToken ct = default)
    {
        Guard.NotNull(vars);

        return RunAsync<JsonValue, ScriptVars>(vars, async (vars, context, ct) =>
        {
            engine.SetValue("complete", new Action<JsValue?>(value =>
            {
                context.Complete(JsonMapper.Map(value));
            }));

            // The evaluation holds the lock, but returns without waiting for the pending promises.
            var result = await RunLockedAsync(() => engine.Evaluate(parsed), ct);

            return await context.WaitForCompletionAsync(() => JsonMapper.Map(result));
        }, ct);
    }

    public ContentData Transform(DataScriptVars vars)
    {
        Guard.NotNull(vars);

        return Run(vars, (vars, _) =>
        {
            ContentData? result = null;

            engine.SetValue("complete", new Action<JsValue?>(_ =>
            {
                result ??= vars.Data;
            }));

            engine.SetValue("replace", new Action(() =>
            {
                if (result == null && TransformData(out var modified))
                {
                    result = modified;
                }
            }));

            engine.Evaluate(parsed);

            return result ?? vars.Data!;
        });
    }

    public ValueTask<ContentData> TransformAsync(DataScriptVars vars,
        CancellationToken ct = default)
    {
        Guard.NotNull(vars);

        return RunAsync<ContentData, DataScriptVars>(vars, async (vars, context, ct) =>
        {
            var data = vars.Data!;

            engine.SetValue("complete", new Action<JsValue?>(_ =>
            {
                if (!context.IsCompleted)
                {
                    context.Complete(data);
                }
            }));

            engine.SetValue("replace", new Action(() =>
            {
                if (!context.IsCompleted && TransformData(out var modified))
                {
                    context.Complete(modified);
                }
            }));

            // The evaluation holds the lock, but returns without waiting for the pending promises.
            await RunLockedAsync(() => engine.Evaluate(parsed), ct);

            return await context.WaitForCompletionAsync(() => data);
        }, ct);
    }

    private T Run<T, TVars>(TVars vars, Func<TVars, ScriptExecutionContext, T> action) where TVars : ScriptVars
    {
        executionGate.Wait();
        try
        {
            // The extensions resolve the variables over the context, therefore it is also needed here.
            var context = new ScriptExecutionContext(engine);
            context.CopyFrom(vars);

            PrepareEngine(vars, default);
            return action(vars, context);
        }
        catch (Exception ex)
        {
            throw MapException(ex);
        }
        finally
        {
            executionGate.Release();
        }
    }

    private async ValueTask<T> RunAsync<T, TVars>(TVars vars, Func<TVars, AsyncScriptExecutionContext<T>, CancellationToken, ValueTask<T>> action,
        CancellationToken ct) where TVars : ScriptVars
    {
        using var combined = CancellationTokenSource.CreateLinkedTokenSource(ct);

        // Enforce a timeout after a configured time span.
        combined.CancelAfter(engineOptions.TimeoutExecution);

        await executionGate.WaitAsync(ct);
        try
        {
            PrepareEngine(vars, combined.Token);

            // The extensions resolve the variables over the context, therefore it is also needed here.
            var context = new AsyncScriptExecutionContext<T>(engine, this, combined.Token);
            context.CopyFrom(vars);

            return await action(vars, context, combined.Token);
        }
        catch (Exception ex)
        {
            throw MapException(ex);
        }
        finally
        {
            // Stop pending tasks before the token source is disposed, they must not touch the engine anymore.
            await combined.CancelAsync();

            // The next execution reuses the engine, therefore we wait until no callback is inside anymore.
            // Callbacks that are still waiting for the lock are rejected, because the token is cancelled by now.
            await engineLock.WaitAsync(default(CancellationToken));
            engineLock.Release();

            executionGate.Release();
        }
    }

    public void Dispose()
    {
        engine.Dispose();
        engineLock.Dispose();
        executionGate.Dispose();
        placeholder.Dispose();
    }

    internal async Task<T> RunLockedAsync<T>(Func<T> action, CancellationToken ct)
    {
        // Only one thread is allowed inside the engine, no matter whether it is an execution or a callback.
        // A callback that is reached synchronously from the engine thread asks for the lock while the
        // evaluation still holds it. That is not a deadlock, because the wait yields instead of blocking and
        // the callback continues on another thread once the evaluation is done. It must not be made
        // reentrant, that would run the promise jobs in the middle of a statement.
        await engineLock.WaitAsync(ct);
        try
        {
            return action();
        }
        finally
        {
            engineLock.Release();
        }
    }

    private bool TransformData(out ContentData result)
    {
        var dataInstance =
            scriptOptions.AsContext ?
                engine.GetValue("ctx").AsObject().Get("data") :
                engine.GetValue("data");

        if (dataInstance != null &&
            dataInstance.IsObject() &&
            dataInstance.AsObject() is ContentDataObject dataObject &&
            dataObject.TryUpdate(out var modified))
        {
            result = modified;
            return true;
        }

        result = null!;
        return false;
    }

    private void PrepareEngine(ScriptVars vars, CancellationToken ct)
    {
        // Removes everything the previous execution has declared, including global const and let.
        engine.Advanced.RestoreGlobalSnapshot(snapshot);
        engine.Constraints.Reset();

        // The engine is created once, therefore the token of the current execution is assigned here.
        // The generic reset above does not clear it, so the callbacks keep the token as well.
        cancellation?.Reset(ct);

        if (scriptOptions.AsContext)
        {
            var contextInstance = new WritableContext(engine, vars);

            engine.SetValue("ctx", contextInstance);
            engine.SetValue("context", contextInstance);
        }
        else
        {
            foreach (var (key, item) in vars)
            {
                engine.SetValue(key, item);
            }
        }
    }

    internal static Exception MapException(Exception inner)
    {
        static Exception BuildException(string errorKey, string message, Exception? inner = null)
        {
            return new ValidationException(T.Get(errorKey, new { message }), inner);
        }

        switch (inner)
        {
            case ArgumentException:
                return BuildException("common.jsParseError", inner.Message);
            case ParseErrorException:
                return BuildException("common.jsError", inner.Message);
            case ScriptPreparationException:
                return BuildException("common.jsError", inner.Message);
            case JavaScriptException:
                return BuildException("common.jsError", inner.Message);
            case JintException:
                return BuildException("common.jsError", inner.Message);
            case DomainException:
                return inner;
            default:
                return BuildException("common.jsError", inner.GetType().Name, inner);
        }
    }
}
