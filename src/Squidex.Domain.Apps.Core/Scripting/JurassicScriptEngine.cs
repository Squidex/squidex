// ==========================================================================
//  JurassicScriptEngine.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using Jurassic;
using Jurassic.Library;
using Newtonsoft.Json;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Infrastructure;

// ReSharper disable InvertIf
// ReSharper disable ConvertToLambdaExpression

namespace Squidex.Domain.Apps.Core.Scripting
{
    public sealed class JurassicScriptEngine : IScriptEngine
    {
        private readonly JsonSerializerSettings serializerSettings;

        public JurassicScriptEngine(JsonSerializerSettings serializerSettings)
        {
            Guard.NotNull(serializerSettings, nameof(serializerSettings));

            this.serializerSettings = serializerSettings;
        }

        public void Execute(ScriptContext context, string script, string operationName)
        {
            Guard.NotNull(context, nameof(context));

            if (!string.IsNullOrWhiteSpace(script))
            {
                var engine = CreateScriptEngine(context);

                EnableDisallow(engine);
                EnableReject(engine, operationName);

                Execute(engine, script, operationName);
            }
        }

        public NamedContentData ExecuteAndTransform(ScriptContext context, string script, string operationName)
        {
            Guard.NotNull(context, nameof(context));

            var result = context.Data;

            if (!string.IsNullOrWhiteSpace(script))
            {
                var engine = CreateScriptEngine(context);

                EnableDisallow(engine);
                EnableReject(engine, operationName);

                engine.SetGlobalFunction("replace", new Action<object>(data =>
                {
                    try
                    {
                        result = JsonConvert.DeserializeObject<NamedContentData>(JSONObject.Stringify(engine, data));
                    }
                    catch
                    {
                        result = new NamedContentData();
                    }
                }));

                Execute(engine, script, operationName);
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

                    engine.SetGlobalFunction("replace", new Action<object>(data =>
                    {
                        result = JsonConvert.DeserializeObject<NamedContentData>(JSONObject.Stringify(engine, data));
                    }));
                }
                catch (Exception)
                {
                    result = context.Data;
                }
            }

            return result;
        }

        private static void Execute(ScriptEngine engine, string script, string operationName)
        {
            try
            {
                engine.Execute(script);
            }
            catch (JavaScriptException ex)
            {
                throw new ValidationException($"Failed to {operationName} with javascript error.", new ValidationError(ex.Message));
            }
        }

        private ScriptEngine CreateScriptEngine(ScriptContext context)
        {
            var engine = new ScriptEngine { ForceStrictMode = true };

            engine.SetGlobalValue("ctx", JSONObject.Parse(engine, JsonConvert.SerializeObject(context, serializerSettings)));

            return engine;
        }

        private static void EnableReject(ScriptEngine engine, string operationName)
        {
            Guard.NotNullOrEmpty(operationName, nameof(operationName));

            engine.SetGlobalFunction("reject", new Action<string>(message =>
            {
                var errors = !string.IsNullOrWhiteSpace(message) ? new[] { new ValidationError(message) } : null;

                throw new ValidationException($"Script rejected to to {operationName}.", errors);
            }));
        }

        private static void EnableDisallow(ScriptEngine engine)
        {
            engine.SetGlobalFunction("disallow", new Action<string>(message =>
            {
                var exMessage = !string.IsNullOrWhiteSpace(message) ? message : "Not allowed";

                throw new DomainForbiddenException(exMessage);
            }));
        }
    }
}
