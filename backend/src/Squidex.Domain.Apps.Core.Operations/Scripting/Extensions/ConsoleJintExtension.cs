// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Text;
using Jint;
using Jint.Native;
using Jint.Native.Json;
using Jint.Runtime.Interop;
using Microsoft.Extensions.Logging;
using Squidex.Domain.Apps.Core.Properties;
using Squidex.Flows;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Core.Scripting.Extensions;

public sealed class ConsoleJintExtension(ILogger<ConsoleJintExtension> log, IScriptLogStore? scriptLogStore = null) : IJintExtension, IScriptDescriptor
{
    private static readonly string[] Levels = ["log", "info", "warn", "error", "debug"];

    public void Extend(Engine engine)
    {
        var console = new JsObject(engine);

        foreach (var level in Levels)
        {
            console.Set(level, new ClrFunction(engine, level, (_, args) =>
            {
                Log(engine, level, args);
                return JsValue.Undefined;
            }));
        }

        engine.SetValue("console", console);
    }

    private void Log(Engine engine, string level, JsValue[] args)
    {
        var context = engine.GetContext();

        var message = Format(engine, args);

        // Rules capture the output per flow step, the output is ignored outside of flows.
        if (!string.IsNullOrWhiteSpace(message))
        {
            FlowConsole.Out(level == "log" ? message : $"{level.ToUpperInvariant()}: {message}");
        }

        var scriptLog = ScriptLog.Current;

        if (scriptLog != null)
        {
            // The log is written once when the scope is completed.
            if (!scriptLog.HasEntries && scriptLog.Persist && scriptLogStore != null && context.TryGetValueIfExists<DomainId>("appId", out var appId))
            {
                scriptLog.OnCompleted(completed => scriptLogStore.Log(appId, completed.Name, completed));
            }

            scriptLog.Add(level, message);
        }

        if (log.IsEnabled(LogLevel.Debug))
        {
            context.TryGetValueIfExists<string>("appName", out var appName);

            log.LogDebug("Script {name} in app {appName} logged with console.{level}: {message}",
                scriptLog?.Name,
                appName,
                level,
                message);
        }
    }

    private static string Format(Engine engine, JsValue[] args)
    {
        var sb = new StringBuilder();

        foreach (var arg in args)
        {
            if (sb.Length > 0)
            {
                sb.Append(' ');
            }

            sb.Append(Format(engine, arg));

            // Protect the memory, the log truncates the message anyway.
            if (sb.Length > ScriptLog.MaxLength)
            {
                break;
            }
        }

        return sb.ToString();
    }

    private static string Format(Engine engine, JsValue value)
    {
        if (value.IsString() || value.IsUndefined() || value.IsNull() || value.IsNumber() || value.IsBoolean())
        {
            return value.ToString();
        }

        try
        {
            var json = new JsonSerializer(engine).Serialize(value);

            return json.IsString() ? json.ToString() : value.ToString();
        }
        catch
        {
            // Cyclic objects cannot be serialized, therefore we fall back to the default representation.
            return value.ToString();
        }
    }

    public void Describe(AddDescription describe, ScriptScope scope)
    {
        describe(JsonType.Object, "console",
            Resources.ScriptingConsole);

        describe(JsonType.Function, "console.log(value)",
            Resources.ScriptingConsoleLog);

        describe(JsonType.Function, "console.info(value)",
            Resources.ScriptingConsoleLog);

        describe(JsonType.Function, "console.warn(value)",
            Resources.ScriptingConsoleLog);

        describe(JsonType.Function, "console.error(value)",
            Resources.ScriptingConsoleLog);

        describe(JsonType.Function, "console.debug(value)",
            Resources.ScriptingConsoleLog);
    }
}
