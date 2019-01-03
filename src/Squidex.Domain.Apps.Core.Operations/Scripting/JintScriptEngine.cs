// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Security.Claims;
using Jint;
using Jint.Native;
using Jint.Native.Date;
using Jint.Native.Object;
using Jint.Runtime;
using Jint.Runtime.Interop;
using NodaTime;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Core.Scripting.ContentWrapper;
using Squidex.Infrastructure;
using Squidex.Shared.Users;

namespace Squidex.Domain.Apps.Core.Scripting
{
    public sealed class JintScriptEngine : IScriptEngine
    {
        public TimeSpan Timeout { get; set; } = TimeSpan.FromMilliseconds(200);

        public sealed class Converter : IObjectConverter
        {
            public Engine Engine { get; set; }

            public bool TryConvert(object value, out JsValue result)
            {
                result = null;

                if (value is Enum)
                {
                    result = value.ToString();
                    return true;
                }

                switch (value)
                {
                    case IUser user:
                        result = JintUser.Create(Engine, user);
                        return true;
                    case ClaimsPrincipal principal:
                        result = JintUser.Create(Engine, principal);
                        return true;
                    case Instant instant:
                        result = JsValue.FromObject(Engine, instant.ToDateTimeUtc());
                        return true;
                    case NamedContentData content:
                        result = new ContentDataObject(Engine, content);
                        return true;
                }

                return false;
            }
        }

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
            catch (ArgumentException ex)
            {
                throw new ValidationException($"Failed to execute script with javascript syntax error: {ex.Message}", new ValidationError(ex.Message));
            }
            catch (JavaScriptException ex)
            {
                throw new ValidationException($"Failed to execute script with javascript error: {ex.Message}", new ValidationError(ex.Message));
            }
        }

        private Engine CreateScriptEngine(ScriptContext context)
        {
            var engine = CreateScriptEngine();

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
                contextInstance.FastAddProperty("user", JintUser.Create(engine, context.User), false, true, false);
            }

            if (!string.IsNullOrWhiteSpace(context.Operation))
            {
                contextInstance.FastAddProperty("operation", context.Operation, false, true, false);
            }

            engine.SetValue("ctx", contextInstance);
            engine.SetValue("context", contextInstance);

            return engine;
        }

        private Engine CreateScriptEngine(IReferenceResolver resolver = null, Dictionary<string, Func<string>> customFormatters = null)
        {
            var converter = new Converter();

            var engine = new Engine(options =>
            {
                if (resolver != null)
                {
                    options.SetReferencesResolver(resolver);
                }

                options.TimeoutInterval(Timeout).Strict().AddObjectConverter(converter);
            });

            if (customFormatters != null)
            {
                foreach (var kvp in customFormatters)
                {
                    engine.SetValue(kvp.Key, Safe(kvp.Value));
                }
            }

            converter.Engine = engine;

            engine.SetValue("slugify", new ClrFunctionInstance(engine, "slugify", Slugify));
            engine.SetValue("formatTime", new ClrFunctionInstance(engine, "formatTime", FormatDate));
            engine.SetValue("formatDate", new ClrFunctionInstance(engine, "formatDate", FormatDate));

            return engine;
        }

        private static Func<string> Safe(Func<string> func)
        {
            return new Func<string>(() =>
            {
                try
                {
                    return func();
                }
                catch
                {
                    return "null";
                }
            });
        }

        private static JsValue Slugify(JsValue thisObject, JsValue[] arguments)
        {
            try
            {
                var stringInput = TypeConverter.ToString(arguments.At(0));
                var single = false;

                if (arguments.Length > 1)
                {
                    single = TypeConverter.ToBoolean(arguments.At(1));
                }

                return stringInput.Slugify(null, single);
            }
            catch
            {
                return JsValue.Undefined;
            }
        }

        private static JsValue FormatDate(JsValue thisObject, JsValue[] arguments)
        {
            try
            {
                var dateValue = ((DateInstance)arguments.At(0)).ToDateTime();
                var dateFormat = TypeConverter.ToString(arguments.At(1));

                return dateValue.ToString(dateFormat, CultureInfo.InvariantCulture);
            }
            catch
            {
                return JsValue.Undefined;
            }
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

                throw new ValidationException("Script rejected the operation.", errors);
            }));
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

        public string Interpolate(string name, object context, string script, Dictionary<string, Func<string>> customFormatters = null)
        {
            try
            {
                var result =
                    CreateScriptEngine(NullPropagation.Instance, customFormatters)
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
    }
}
