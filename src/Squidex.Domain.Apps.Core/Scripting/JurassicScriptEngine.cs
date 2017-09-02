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
                var engine = CreateScriptEngine(context, operationName);

                Execute(script, operationName, engine, true);
            }
        }

        public NamedContentData ExecuteAndTransform(ScriptContext context, string script, string operationName, bool failOnError = false)
        {
            Guard.NotNull(context, nameof(context));

            var result = context.Data;

            if (!string.IsNullOrWhiteSpace(script))
            {
                var engine = CreateScriptEngine(context, operationName);

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

                Execute(script, operationName, engine, failOnError);
            }

            return result;
        }

        private static void Execute(string script, string operationName, ScriptEngine engine, bool failOnError = false)
        {
            try
            {
                engine.Execute(script);
            }
            catch (JavaScriptException ex)
            {
                if (failOnError)
                {
                    throw new ValidationException($"Failed to {operationName} with javascript error.", new ValidationError(ex.Message));
                }
            }
        }

        private ScriptEngine CreateScriptEngine(ScriptContext context, string operationName)
        {
            Guard.NotNullOrEmpty(operationName, nameof(operationName));

            var engine = new ScriptEngine { ForceStrictMode = true };

            engine.SetGlobalFunction("disallow", new Action<string>(message =>
            {
                var exMessage = !string.IsNullOrWhiteSpace(message) ? message : "Not allowed";

                throw new DomainForbiddenException(exMessage);
            }));

            engine.SetGlobalFunction("reject", new Action<string>(message =>
            {
                var errors = !string.IsNullOrWhiteSpace(message) ? new[] { new ValidationError(message) } : null;

                throw new ValidationException($"Script rejected to to {operationName}.", errors);
            }));

            var json = JsonConvert.SerializeObject(context, serializerSettings);

            engine.SetGlobalValue("ctx", JSONObject.Parse(engine, json));

            return engine;
        }
    }
}
