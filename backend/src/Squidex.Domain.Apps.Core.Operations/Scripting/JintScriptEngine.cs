// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Esprima;
using Jint;
using Jint.Native;
using Jint.Runtime;
using Jint.Runtime.Interop;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Core.Scripting.ContentWrapper;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Json.Objects;
using Squidex.Infrastructure.Validation;

namespace Squidex.Domain.Apps.Core.Scripting
{
    public sealed class JintScriptEngine : IScriptEngine
    {
        private readonly IHttpClientFactory? httpClientFactory;

        public TimeSpan Timeout { get; set; } = TimeSpan.FromMilliseconds(200);

        public JintScriptEngine(IHttpClientFactory? httpClientFactory = null)
        {
            this.httpClientFactory = httpClientFactory;
        }

        public void Execute(ScriptContext context, string script)
        {
            Guard.NotNull(context);

            if (!string.IsNullOrWhiteSpace(script))
            {
                var engine =
                    CreateScriptEngine()
                        .AddContext(context)
                        .AddDisallow()
                        .AddReject();

                Execute(engine, script);
            }
        }

        public NamedContentData ExecuteAndTransform(ScriptContext context, string script)
        {
            Guard.NotNull(context);

            var result = context.Data!;

            if (!string.IsNullOrWhiteSpace(script))
            {
                var engine =
                    CreateScriptEngine()
                        .AddContext(context)
                        .AddDisallow()
                        .AddReject();

                engine.SetValue("replace", new Action(() =>
                {
                    var dataInstance = engine.GetValue("ctx").AsObject().Get("data");

                    if (dataInstance != null && dataInstance.IsObject() && dataInstance.AsObject() is ContentDataObject data)
                    {
                        data.TryUpdate(out result);
                    }
                }));

                Execute(engine, script);
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
                    var engine =
                        CreateScriptEngine()
                            .AddContext(context);

                    engine.SetValue("replace", new Action(() =>
                    {
                        var dataInstance = engine.GetValue("ctx").AsObject().Get("data");

                        if (dataInstance != null && dataInstance.IsObject() && dataInstance.AsObject() is ContentDataObject data)
                        {
                            data.TryUpdate(out result);
                        }
                    }));

                    engine.Execute(script);
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

        private Engine CreateScriptEngine(IReferenceResolver? resolver = null)
        {
            var engine = new Engine(options =>
            {
                if (resolver != null)
                {
                    options.SetReferencesResolver(resolver);
                }

                options.TimeoutInterval(Timeout).Strict().AddObjectConverter(DefaultConverter.Instance);
            });

            engine.AddHelpers();

            return engine;
        }

        public bool Evaluate(string name, object context, string script)
        {
            try
            {
                var result =
                    CreateScriptEngine(NullPropagation.Instance)
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

        public string? Interpolate(string name, object context, string script, Dictionary<string, Func<string>>? customFormatters = null)
        {
            try
            {
                var result =
                    CreateScriptEngine(NullPropagation.Instance)
                        .AddFormatters(customFormatters)
                        .SetValue(name, context)
                        .Execute(script)
                        .GetCompletionValue()
                        .ToObject();

                var converted = result.ToString();

                return converted == "undefined" ? "null" : converted;
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        public Task<IJsonValue> GetAsync(ScriptContext context, string script)
        {
            using (var cts = new CancellationTokenSource(Timeout))
            {
                var tcs = new TaskCompletionSource<IJsonValue>();

                using (cts.Token.Register(() =>
                {
                    tcs.SetCanceled();
                }))
                {
                    var engine =
                        CreateScriptEngine()
                            .AddContext(context);

                    if (httpClientFactory != null)
                    {
                        var http = new JintHttp(httpClientFactory, cts.Token, tcs.SetException);

                        http.Add(engine);
                    }

                    engine.SetValue("complete", new Action<JsValue?>(value =>
                    {
                        tcs.SetResult(JsonMapper.Map(value));
                    }));

                    engine.Execute(script);
                }

                return tcs.Task;
            }
        }
    }
}
