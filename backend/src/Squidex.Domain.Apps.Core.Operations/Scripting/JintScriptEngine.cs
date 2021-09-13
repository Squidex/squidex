// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Diagnostics;
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
using Squidex.Infrastructure.Translations;
using Squidex.Infrastructure.Validation;

namespace Squidex.Domain.Apps.Core.Scripting
{
    public sealed class JintScriptEngine : IScriptEngine
    {
        private readonly IJintExtension[] extensions;
        private readonly Parser parser;

        public TimeSpan TimeoutScript { get; set; } = TimeSpan.FromMilliseconds(200);

        public TimeSpan TimeoutExecution { get; set; } = TimeSpan.FromMilliseconds(4000);

        private TimeSpan ActualTimeoutScript
        {
            get
            {
                if (Debugger.IsAttached)
                {
                    return TimeSpan.FromHours(1);
                }

                return TimeoutScript;
            }
        }

        private TimeSpan ActualTimeoutExecution
        {
            get
            {
                if (Debugger.IsAttached)
                {
                    return TimeSpan.FromHours(1);
                }

                return TimeoutExecution;
            }
        }

        public JintScriptEngine(IMemoryCache cache, IEnumerable<IJintExtension>? extensions = null)
        {
            parser = new Parser(cache);

            this.extensions = extensions?.ToArray() ?? Array.Empty<IJintExtension>();
        }

        public async Task<IJsonValue> ExecuteAsync(ScriptVars vars, string script, ScriptOptions options = default,
            CancellationToken ct = default)
        {
            Guard.NotNull(vars, nameof(vars));
            Guard.NotNullOrEmpty(script, nameof(script));

            using (var cts = new CancellationTokenSource(ActualTimeoutExecution))
            {
                using (var combined = CancellationTokenSource.CreateLinkedTokenSource(cts.Token, ct))
                {
                    var tcs = new TaskCompletionSource<IJsonValue>();

                    await using (combined.Token.Register(() => tcs.TrySetCanceled(combined.Token)))
                    {
                        var context =
                            CreateEngine(options)
                                .Extend(vars, options)
                                .Extend(extensions)
                                .ExtendAsync(extensions, tcs.TrySetException, combined.Token);

                        context.Engine.SetValue("complete", new Action<JsValue?>(value =>
                        {
                            tcs.TrySetResult(JsonMapper.Map(value));
                        }));

                        Execute(context.Engine, script);

                        if (!context.IsAsync)
                        {
                            tcs.TrySetResult(JsonMapper.Map(context.Engine.GetCompletionValue()));
                        }

                        return await tcs.Task;
                    }
                }
            }
        }

        public async Task<ContentData> TransformAsync(ScriptVars vars, string script, ScriptOptions options = default,
            CancellationToken ct = default)
        {
            Guard.NotNull(vars, nameof(vars));
            Guard.NotNullOrEmpty(script, nameof(script));

            using (var cts = new CancellationTokenSource(ActualTimeoutExecution))
            {
                using (var combined = CancellationTokenSource.CreateLinkedTokenSource(cts.Token, ct))
                {
                    var tcs = new TaskCompletionSource<ContentData>();

                    await using (combined.Token.Register(() => tcs.TrySetCanceled(combined.Token)))
                    {
                        var context =
                            CreateEngine(options)
                                .Extend(vars, options)
                                .Extend(extensions)
                                .ExtendAsync(extensions, tcs.TrySetException, combined.Token);

                        context.Engine.SetValue("complete", new Action<JsValue?>(_ =>
                        {
                            tcs.TrySetResult(vars.Data!);
                        }));

                        context.Engine.SetValue("replace", new Action(() =>
                        {
                            var dataInstance = context.Engine.GetValue("ctx").AsObject().Get("data");

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

                        Execute(context.Engine, script);

                        if (!context.IsAsync)
                        {
                            tcs.TrySetResult(vars.Data!);
                        }

                        return await tcs.Task;
                    }
                }
            }
        }

        public IJsonValue Execute(ScriptVars vars, string script, ScriptOptions options = default)
        {
            Guard.NotNull(vars, nameof(vars));
            Guard.NotNullOrEmpty(script, nameof(script));

            var context =
                CreateEngine(options)
                    .Extend(vars, options)
                    .Extend(extensions);

            Execute(context.Engine, script);

            return JsonMapper.Map(context.Engine.GetCompletionValue());
        }

        private ExecutionContext CreateEngine(ScriptOptions options)
        {
            var engine = new Engine(engineOptions =>
            {
                engineOptions.AddObjectConverter(DefaultConverter.Instance);
                engineOptions.SetReferencesResolver(NullPropagation.Instance);
                engineOptions.Strict();
                engineOptions.TimeoutInterval(ActualTimeoutScript);
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

            var context = new ExecutionContext(engine);

            return context;
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
                throw new ValidationException(T.Get("common.jsParseError", new { error = ex.Message }));
            }
            catch (JavaScriptException ex)
            {
                throw new ValidationException(T.Get("common.jsError", new { message = ex.Message }));
            }
            catch (ParserException ex)
            {
                throw new ValidationException(T.Get("common.jsError", new { message = ex.Message }));
            }
            catch (DomainException)
            {
                throw;
            }
            catch
            {
                throw new ValidationException(T.Get("common.jsError", new { message = "RuntimeError" }));
            }
        }
    }
}
