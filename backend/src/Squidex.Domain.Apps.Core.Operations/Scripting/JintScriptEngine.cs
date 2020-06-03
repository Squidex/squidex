// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Esprima;
using Jint;
using Jint.Native;
using Jint.Runtime;
using Microsoft.Extensions.Caching.Memory;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Core.Scripting.ContentWrapper;
using Squidex.Domain.Apps.Core.Scripting.Internal;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Json.Objects;
using Squidex.Infrastructure.Validation;

namespace Squidex.Domain.Apps.Core.Scripting
{
    public sealed class JintScriptEngine : IScriptEngine
    {
        private readonly IJintExtension[] extensions;
        private readonly Parser parser;

        public TimeSpan Timeout { get; set; } = TimeSpan.FromMilliseconds(200);

        public TimeSpan ExecutionTimeout { get; set; } = TimeSpan.FromMilliseconds(4000);

        public JintScriptEngine(IMemoryCache memoryCache, IEnumerable<IJintExtension>? extensions = null)
        {
            parser = new Parser(memoryCache);

            this.extensions = extensions?.ToArray() ?? Array.Empty<IJintExtension>();
        }

        public async Task ExecuteAsync(ScriptVars vars, string script)
        {
            Guard.NotNull(vars, nameof(vars));
            Guard.NotNullOrEmpty(script, nameof(script));

            using (var cts = new CancellationTokenSource(ExecutionTimeout))
            {
                var tcs = new TaskCompletionSource<bool>();

                using (cts.Token.Register(() => tcs.TrySetCanceled()))
                {
                    var engine =
                        CreateEngine(vars, true, cts.Token, tcs.TrySetException, true)
                            .AddDisallow()
                            .AddReject();

                    engine.SetValue("complete", new Action<JsValue?>(value =>
                    {
                        tcs.TrySetResult(true);
                    }));

                    Execute(engine, script);

                    if (engine.GetValue("async") != true)
                    {
                        tcs.TrySetResult(true);
                    }

                    await tcs.Task;
                }
            }
        }

        public async Task<NamedContentData> ExecuteAndTransformAsync(ScriptVars vars, string script)
        {
            Guard.NotNull(vars, nameof(vars));
            Guard.NotNullOrEmpty(script, nameof(script));

            using (var cts = new CancellationTokenSource(ExecutionTimeout))
            {
                var tcs = new TaskCompletionSource<NamedContentData>();

                using (cts.Token.Register(() => tcs.TrySetCanceled()))
                {
                    var engine =
                        CreateEngine(vars, true, cts.Token, tcs.TrySetException, true)
                            .AddDisallow()
                            .AddReject();

                    engine.SetValue("complete", new Action<JsValue?>(value =>
                    {
                        tcs.TrySetResult(vars.Data!);
                    }));

                    engine.SetValue("replace", new Action(() =>
                    {
                        var dataInstance = engine.GetValue("ctx").AsObject().Get("data");

                        if (dataInstance != null && dataInstance.IsObject() && dataInstance.AsObject() is ContentDataObject data)
                        {
                            if (!tcs.Task.IsCompleted)
                            {
                                if (data.TryUpdate(out var modified))
                                {
                                    tcs.TrySetResult(modified);
                                }
                                else
                                {
                                    tcs.TrySetResult(vars.Data!);
                                }
                            }
                        }
                    }));

                    Execute(engine, script);

                    if (engine.GetValue("async") != true)
                    {
                        tcs.TrySetResult(vars.Data!);
                    }

                    return await tcs.Task;
                }
            }
        }

        public async Task<NamedContentData> TransformAsync(ScriptVars vars, string script)
        {
            Guard.NotNull(vars, nameof(vars));
            Guard.NotNullOrEmpty(script, nameof(script));

            using (var cts = new CancellationTokenSource(ExecutionTimeout))
            {
                var tcs = new TaskCompletionSource<NamedContentData>();

                using (cts.Token.Register(() => tcs.TrySetCanceled()))
                {
                    var engine = CreateEngine(vars, true, cts.Token, tcs.TrySetException, true);

                    engine.SetValue("complete", new Action<JsValue?>(value =>
                    {
                        tcs.TrySetResult(vars.Data!);
                    }));

                    engine.SetValue("replace", new Action(() =>
                    {
                        var dataInstance = engine.GetValue("ctx").AsObject().Get("data");

                        if (dataInstance != null && dataInstance.IsObject() && dataInstance.AsObject() is ContentDataObject data)
                        {
                            if (!tcs.Task.IsCompleted)
                            {
                                if (data.TryUpdate(out var modified))
                                {
                                    tcs.TrySetResult(modified);
                                }
                                else
                                {
                                    tcs.TrySetResult(vars.Data!);
                                }
                            }
                        }
                    }));

                    Execute(engine, script);

                    if (engine.GetValue("async") != true)
                    {
                        tcs.TrySetResult(vars.Data!);
                    }

                    return await tcs.Task;
                }
            }
        }

        public bool Evaluate(ScriptVars vars, string script)
        {
            Guard.NotNull(vars, nameof(vars));
            Guard.NotNullOrEmpty(script, nameof(script));

            try
            {
                var engine = CreateEngine(vars, false);

                Execute(engine, script);

                var converted = Equals(engine.GetCompletionValue().ToObject(), true);

                return converted;
            }
            catch
            {
                return false;
            }
        }

        public string? Interpolate(ScriptVars vars, string script)
        {
            Guard.NotNull(vars, nameof(vars));
            Guard.NotNullOrEmpty(script, nameof(script));

            try
            {
                var engine = CreateEngine(vars, false);

                Execute(engine, script);

                var converted = engine.GetCompletionValue().ToObject()?.ToString() ?? "null";

                return converted == "undefined" ? "null" : converted;
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        public Task<IJsonValue> GetAsync(ScriptVars vars, string script)
        {
            Guard.NotNull(vars, nameof(vars));
            Guard.NotNullOrEmpty(script, nameof(script));

            using (var cts = new CancellationTokenSource(ExecutionTimeout))
            {
                var tcs = new TaskCompletionSource<IJsonValue>();

                using (cts.Token.Register(() =>
                {
                    tcs.TrySetCanceled();
                }))
                {
                    var engine = CreateEngine(vars, true, cts.Token, ex => tcs.TrySetException(ex), true);

                    engine.SetValue("complete", new Action<JsValue?>(value =>
                    {
                        tcs.TrySetResult(JsonMapper.Map(value));
                    }));

                    engine.Execute(script);

                    if (engine.GetValue("async") != true)
                    {
                        tcs.TrySetResult(JsonMapper.Map(engine.GetCompletionValue()));
                    }
                }

                return tcs.Task;
            }
        }

        private Engine CreateEngine(ScriptVars vars, bool nested, CancellationToken cancellationToken = default, ExceptionHandler? exceptionHandler = null, bool async = false)
        {
            var engine = new Engine(options =>
            {
                options.AddObjectConverter(DefaultConverter.Instance);
                options.SetReferencesResolver(NullPropagation.Instance);
                options.Strict();
                options.TimeoutInterval(Timeout);
            });

            if (async)
            {
                engine.SetValue("async", false);
            }

            foreach (var extension in extensions)
            {
                extension.Extend(engine);
            }

            var executionvars = new ExecutionContext(engine, cancellationToken, exceptionHandler);

            vars.Add(executionvars, nested);

            foreach (var extension in extensions)
            {
                extension.Extend(executionvars, async);
            }

            return executionvars.Engine;
        }

        private void Execute(Engine engine, string script)
        {
            try
            {
                var program = parser.Parse(script);

                engine.Execute(program);
            }
            catch (ArgumentException ex)
            {
                throw new ValidationException($"Failed to execute script with javascript syntax error: {ex.Message}", new ValidationError(ex.Message));
            }
            catch (JavaScriptException ex)
            {
                throw new ValidationException($"Failed to execute script with javascript error: {ex.Message}", new ValidationError(ex.Message));
            }
            catch (ParserException ex)
            {
                throw new ValidationException($"Failed to execute script with javascript error: {ex.Message}", new ValidationError(ex.Message));
            }
        }
    }
}
