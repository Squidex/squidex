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
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Core.Scripting.ContentWrapper;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Json.Objects;
using Squidex.Infrastructure.Validation;

namespace Squidex.Domain.Apps.Core.Scripting
{
    public sealed class JintScriptEngine : IScriptEngine
    {
        private readonly IScriptExtension[] extensions;

        public TimeSpan Timeout { get; set; } = TimeSpan.FromMilliseconds(200);

        public TimeSpan ExecutionTimeout { get; set; } = TimeSpan.FromMilliseconds(4000);

        public JintScriptEngine(IEnumerable<IScriptExtension>? extensions = null)
        {
            this.extensions = extensions?.ToArray() ?? Array.Empty<IScriptExtension>();
        }

        public void Execute(ScriptContext context, string script)
        {
            Guard.NotNull(context);

            if (!string.IsNullOrWhiteSpace(script))
            {
                var ctx =
                    CreateScriptEngine()
                        .AddContext(context)
                        .AddDisallow()
                        .AddReject();

                Execute(ctx.Engine, script);
            }
        }

        public NamedContentData ExecuteAndTransform(ScriptContext context, string script)
        {
            Guard.NotNull(context);

            var result = context.Data!;

            if (!string.IsNullOrWhiteSpace(script))
            {
                var ctx =
                    CreateScriptEngine()
                        .AddContext(context)
                        .AddDisallow()
                        .AddReject();

                ctx.Engine.SetValue("replace", new Action(() =>
                {
                    var dataInstance = ctx.Engine.GetValue("ctx").AsObject().Get("data");

                    if (dataInstance != null && dataInstance.IsObject() && dataInstance.AsObject() is ContentDataObject data)
                    {
                        data.TryUpdate(out result);
                    }
                }));

                Execute(ctx.Engine, script);
            }

            return result;
        }

        public NamedContentData Transform(ScriptContext context, string script)
        {
            Guard.NotNull(context);

            var result = context.Data!;

            if (!string.IsNullOrWhiteSpace(script))
            {
                try
                {
                    var ctx =
                        CreateScriptEngine()
                            .AddContext(context);

                    ctx.Engine.SetValue("replace", new Action(() =>
                    {
                        var dataInstance = ctx.Engine.GetValue("ctx").AsObject().Get("data");

                        if (dataInstance != null && dataInstance.IsObject() && dataInstance.AsObject() is ContentDataObject data)
                        {
                            data.TryUpdate(out result);
                        }
                    }));

                    ctx.Engine.Execute(script);
                }
                catch (Exception)
                {
                    result = context.Data!;
                }
            }

            return result;
        }

        private static void Execute(Engine engine, string script)
        {
            try
            {
                engine.Execute(script);
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

        public bool Evaluate(string name, object context, string script)
        {
            try
            {
                var result =
                    CreateScriptEngine()
                        .SetValue(name, context).Engine
                        .SetValue(name, context)
                        .Execute(script)
                        .GetCompletionValue()
                        .ToObject();

                return (bool)result;
            }
            catch
            {
                return false;
            }
        }

        public string? Interpolate(string name, object context, string script)
        {
            try
            {
                var result =
                    CreateScriptEngine()
                        .SetValue(name, context).Engine
                        .SetValue(name, context)
                        .Execute(script)
                        .GetCompletionValue()
                        .ToObject();

                var converted = result?.ToString() ?? "null";

                return converted == "undefined" ? "null" : converted;
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        public Task<IJsonValue> GetAsync(ScriptContext context, string script)
        {
            if (string.IsNullOrWhiteSpace(script))
            {
                return Task.FromResult(JsonValue.Null);
            }

            using (var cts = new CancellationTokenSource(ExecutionTimeout))
            {
                var tcs = new TaskCompletionSource<IJsonValue>();

                using (cts.Token.Register(() =>
                {
                    tcs.TrySetCanceled();
                }))
                {
                    var ctx =
                        CreateScriptEngine(cts.Token, ex => tcs.TrySetException(ex), true)
                            .AddContext(context);

                    ctx.Engine.SetValue("complete", new Action<JsValue?>(value =>
                    {
                        tcs.TrySetResult(JsonMapper.Map(value));
                    }));

                    ctx.Engine.Execute(script);

                    if (ctx.Engine.GetValue("async") != true)
                    {
                        tcs.TrySetResult(JsonMapper.Map(ctx.Engine.GetCompletionValue()));
                    }
                }

                return tcs.Task;
            }
        }

        private ExecutionContext CreateScriptEngine(CancellationToken cancellationToken = default, Action<Exception>? exceptionHandler = null, bool async = false)
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

            var context = new ExecutionContext(engine, cancellationToken, exceptionHandler);

            foreach (var extension in extensions)
            {
                extension.Extend(context, async);
            }

            return context;
        }
    }
}
