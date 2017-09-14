// ==========================================================================
//  JintScriptEngine.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using Jint;
using Jint.Native.Object;
using Jint.Parser;
using Jint.Runtime;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Core.Scripting.ContentWrapper;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Core.Scripting
{
    public sealed class JintScriptEngine : IScriptEngine
    {
        public TimeSpan Timeout { get; set; } = TimeSpan.FromMilliseconds(200);

        public void Execute(ScriptContext context, string script)
        {
            Guard.NotNull(context, nameof(context));

            if (!string.IsNullOrWhiteSpace(script))
            {
                var engine = CreateScriptEngine(context);

                EnableDisallow(engine);
                EnableReject(engine);

                Execute(engine, script);
            }
        }

        public NamedContentData ExecuteAndTransform(ScriptContext context, string script)
        {
            Guard.NotNull(context, nameof(context));

            var result = context.Data;

            if (!string.IsNullOrWhiteSpace(script))
            {
                var engine = CreateScriptEngine(context);

                EnableDisallow(engine);
                EnableReject(engine);

                engine.SetValue("operation", new Action(() =>
                {
                    var dataInstance = engine.GetValue("ctx").AsObject().Get("data");

                    if (dataInstance != null && dataInstance.IsObject() && dataInstance.AsObject() is ContentDataObject data)
                    {
                        data.TryUpdate(out result);
                    }
                }));

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
            Guard.NotNull(context, nameof(context));

            var result = context.Data;

            if (!string.IsNullOrWhiteSpace(script))
            {
                try
                {
                    var engine = CreateScriptEngine(context);

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
                    result = context.Data;
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
            catch (ParserException ex)
            {
                throw new ValidationException($"Failed to execute script with javascript syntaxs error.", new ValidationError(ex.Message));
            }
            catch (JavaScriptException ex)
            {
                throw new ValidationException($"Failed to execute script with javascript error.", new ValidationError(ex.Message));
            }
        }

        private Engine CreateScriptEngine(ScriptContext context)
        {
            var engine = new Engine(options => options.TimeoutInterval(Timeout).Strict());

            var contextInstance = new ObjectInstance(engine);

            if (context.Data != null)
            {
                contextInstance.FastAddProperty("data", new ContentDataObject(engine, context.Data), true, true, true);
            }

            if (context.OldData != null)
            {
                contextInstance.FastAddProperty("oldData", new ContentDataObject(engine, context.OldData), true, true, true);
            }

            if (context.User != null)
            {
                contextInstance.FastAddProperty("user", new JintUser(engine, context.User), false, true, false);
            }

            if (!string.IsNullOrWhiteSpace(context.Operation))
            {
                contextInstance.FastAddProperty("operation", context.Operation, false, true, false);
            }

            engine.SetValue("ctx", contextInstance);

            return engine;
        }

        private static void EnableDisallow(Engine engine)
        {
            engine.SetValue("disallow", new Action<string>(message =>
            {
                var exMessage = !string.IsNullOrWhiteSpace(message) ? message : "Not allowed";

                throw new DomainForbiddenException(exMessage);
            }));
        }

        private static void EnableReject(Engine engine)
        {
            engine.SetValue("reject", new Action<string>(message =>
            {
                var errors = !string.IsNullOrWhiteSpace(message) ? new[] { new ValidationError(message) } : null;

                throw new ValidationException($"Script rejected the operation.", errors);
            }));
        }
    }
}
