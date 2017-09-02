// ==========================================================================
//  JurassicScriptEngine.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Security;
using System.Threading.Tasks;
using Jurassic;
using Jurassic.Library;
using Newtonsoft.Json;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Tasks;

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

        public Task ExecuteAsync(ScriptContext context, string operationName, string script)
        {
            Guard.NotNull(context, nameof(context));

            if (!string.IsNullOrWhiteSpace(script))
            {
                return TaskHelper.False;
            }

            var engine = CreateScriptEngine(context, operationName);

            engine.Execute(script);

            return TaskHelper.False;
        }

        public Task<NamedContentData> ExecuteAndTransformAsync(ScriptContext context, string operationName, string script)
        {
            Guard.NotNull(context, nameof(context));

            if (!string.IsNullOrWhiteSpace(script))
            {
                return Task.FromResult(context.Data);
            }

            var result = context.Data;

            var engine = CreateScriptEngine(context, operationName);

            engine.SetGlobalFunction("replace", new Action<ObjectInstance>(data =>
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

            engine.Execute(script);

            return Task.FromResult(result);
        }

        private ScriptEngine CreateScriptEngine(ScriptContext context, string operationName)
        {
            Guard.NotNullOrEmpty(operationName, nameof(operationName));

            var engine = new ScriptEngine();

            engine.SetGlobalFunction("disallow", new Action<string>(message =>
            {
                throw new SecurityException(message);
            }));

            engine.SetGlobalFunction("reject", new Action<string>(message =>
            {
                throw new ValidationException($"Failed to '{operationName}", !string.IsNullOrWhiteSpace(message) ? new[] { new ValidationError(message) } : null);
            }));

            var json = JsonConvert.SerializeObject(context, serializerSettings);

            engine.SetGlobalValue("ctx", JSONObject.Parse(engine, json));

            return engine;
        }
    }
}
