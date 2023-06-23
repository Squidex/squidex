﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Diagnostics;
using System.Linq.Expressions;
using Esprima;
using Jint;
using Jint.Native;
using Jint.Runtime;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Core.Properties;
using Squidex.Domain.Apps.Core.Scripting.ContentWrapper;
using Squidex.Domain.Apps.Core.Scripting.Internal;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Json.Objects;
using Squidex.Infrastructure.Translations;
using Squidex.Infrastructure.Validation;

namespace Squidex.Domain.Apps.Core.Scripting;

public sealed class JintScriptEngine : IScriptEngine, IScriptDescriptor
{
    private readonly IJintExtension[] extensions;
    private readonly Parser parser;
    private readonly TimeSpan timeoutScript;
    private readonly TimeSpan timeoutExecution;

    public JintScriptEngine(IMemoryCache cache, IOptions<JintScriptOptions> options, IEnumerable<IJintExtension>? extensions = null)
    {
        parser = new Parser(cache);

        timeoutScript = options.Value.TimeoutScript;
        timeoutExecution = options.Value.TimeoutExecution;

        this.extensions = extensions?.ToArray() ?? Array.Empty<IJintExtension>();
    }

    public async Task<JsonValue> ExecuteAsync(ScriptVars vars, string script, ScriptOptions options = default,
        CancellationToken ct = default)
    {
        Guard.NotNull(vars);
        Guard.NotNullOrEmpty(script);

        using var combined = CancellationTokenSource.CreateLinkedTokenSource(ct);
        try
        {
            // Enforce a timeout after a configured time span.
            combined.CancelAfter(timeoutExecution);

            var context =
                CreateEngine<JsonValue?>(options, combined.Token)
                    .Extend(vars, options)
                    .Extend(extensions)
                    .ExtendAsync(extensions);

            context.Engine.SetValue("complete", new Action<JsValue?>(value =>
            {
                context.Complete(JsonMapper.Map(value));
            }));

            var result = Execute(context.Engine, script);

            return await context.CompleteAsync() ?? JsonMapper.Map(result);
        }
        catch (Exception ex)
        {
            throw MapException(ex);
        }
    }

    public async Task<ContentData> TransformAsync(DataScriptVars vars, string script, ScriptOptions options = default,
        CancellationToken ct = default)
    {
        Guard.NotNull(vars);
        Guard.NotNullOrEmpty(script);

        using var combined = CancellationTokenSource.CreateLinkedTokenSource(ct);
        try
        {
            // Enforce a timeout after a configured time span.
            combined.CancelAfter(timeoutExecution);

            var context =
                    CreateEngine<ContentData>(options, combined.Token)
                        .Extend(vars, options)
                        .Extend(extensions)
                        .ExtendAsync(extensions);

            context.Engine.SetValue("complete", new Action<JsValue?>(_ =>
            {
                context.Complete(vars.Data!);
            }));

            context.Engine.SetValue("replace", new Action(() =>
            {
                var dataInstance = context.Engine.GetValue("ctx").AsObject().Get("data");

                if (dataInstance != null && dataInstance.IsObject() && dataInstance.AsObject() is ContentDataObject data)
                {
                    if (!context.IsCompleted && data.TryUpdate(out var modified))
                    {
                        context.Complete(modified);
                    }
                }
            }));

            Execute(context.Engine, script);

            return await context.CompleteAsync() ?? vars.Data!;
        }
        catch (Exception ex)
        {
            throw MapException(ex);
        }
    }

    public JsonValue Execute(ScriptVars vars, string script, ScriptOptions options = default)
    {
        Guard.NotNull(vars);
        Guard.NotNullOrEmpty(script);

        try
        {
            var context =
                CreateEngine<object>(options, default)
                    .Extend(vars, options)
                    .Extend(extensions);

            var result = Execute(context.Engine, script);

            return JsonMapper.Map(result);
        }
        catch (Exception ex)
        {
            throw MapException(ex);
        }
    }

    private ScriptExecutionContext<T> CreateEngine<T>(ScriptOptions options, CancellationToken ct)
    {
        if (Debugger.IsAttached)
        {
            ct = default;
        }

        var engine = new Engine(engineOptions =>
        {
            engineOptions.AddObjectConverter(JintObjectConverter.Instance);
            engineOptions.SetReferencesResolver(NullPropagation.Instance);
            engineOptions.Strict();

            if (!Debugger.IsAttached)
            {
                engineOptions.TimeoutInterval(timeoutScript);
                engineOptions.CancellationToken(ct);
            }
        });

        if (options.CanDisallow)
        {
            engine.AddDisallow();
        }

        if (options.CanReject)
        {
            engine.AddReject();
        }

        foreach (var extension in extensions)
        {
            extension.Extend(engine);
        }

        return new ScriptExecutionContext<T>(engine, ct);
    }

    private JsValue Execute(Engine engine, string script)
    {
        var program = parser.Parse(script);

        lock (engine)
        {
            return engine.Evaluate(program);
        }
    }

    private static Exception MapException(Exception inner)
    {
        switch (inner)
        {
            case ArgumentException:
                return new ValidationException(T.Get("common.jsParseError", new { error = inner.Message }));
            case JavaScriptException:
                return new ValidationException(T.Get("common.jsError", new { message = inner.Message }));
            case ParserException:
                return new ValidationException(T.Get("common.jsError", new { message = inner.Message }));
            case DomainException:
                return inner;
            default:
                return new ValidationException(T.Get("common.jsError", new { message = inner.GetType().Name }), inner);
        }
    }

    public void Describe(AddDescription describe, ScriptScope scope)
    {
        if (scope.HasFlag(ScriptScope.ContentTrigger) || scope.HasFlag(ScriptScope.AssetTrigger))
        {
            return;
        }

        if (scope.HasFlag(ScriptScope.Transform) || scope.HasFlag(ScriptScope.ContentScript))
        {
            describe(JsonType.Function, "replace()",
                Resources.ScriptingReplace);
        }

        describe(JsonType.Function, "disallow(reason)",
            Resources.ScriptingDisallow);

        describe(JsonType.Function, "reject(reason)",
            Resources.ScriptingReject);

        describe(JsonType.Function, "complete()",
            Resources.ScriptingComplete);
    }
}
