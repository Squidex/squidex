// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Core.Scripting;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Json.Objects;

namespace Squidex.Domain.Apps.Entities.Contents.Text.Extraction;

public sealed class ScriptTextExtractionStrategy(IScriptEngine scriptEngine) : ITextExtractionStrategy
{
    private static readonly object ScriptKey = new object();
    private static readonly ScriptOptions ScriptOptions = new ScriptOptions
    {
        AsContext = true,
        CanDisallow = false,
        CanReject = false,
        Readonly = true,
    };

    // The script can override the schema, therefore it must run first.
    public int Order => -1000;

    public ExtractedTexts? Extract(TextExtractionContext context, DomainId contentId, ContentData data)
    {
        var script = context.Schema?.Scripts.Index;
        if (string.IsNullOrWhiteSpace(script))
        {
            return null;
        }

        // The context is only used by one thread, therefore the script can be reused for all contents of the schema.
        var compiled = context.GetOrAdd(ScriptKey, (ScriptEngine: scriptEngine, Script: script), static x => Compile(x.ScriptEngine, x.Script));
        if (compiled == null)
        {
            return null;
        }

        try
        {
            // Script vars are just wrappers over dictionaries for better performance.
            var vars = new ContentScriptVars
            {
                AppId = context.AppId.Id,
                AppName = context.AppId.Name,
                ContentId = contentId,
                Data = data,
                SchemaId = context.SchemaId.Id,
                SchemaName = context.SchemaId.Name,
            };

            return ToTexts(compiled.Execute(vars));
        }
        catch
        {
            // The state of the script is unknown after an error, e.g. a timeout, therefore it is not reused.
            context.Remove(ScriptKey);

            // Fallback to the next strategy.
            return null;
        }
    }

    private static IScript? Compile(IScriptEngine scriptEngine, string script)
    {
        try
        {
            return scriptEngine.CreateScript(script, ScriptOptions);
        }
        catch
        {
            // The contents are indexed without the script.
            return null;
        }
    }

    public static ExtractedTexts? ToTexts(JsonValue result)
    {
        // The script output is used as it is, therefore the normalizers are not applied.
        using var collector = new TextCollector();

        // Undefined or null means that the script wants to fallback to the next strategy.
        switch (result.Value)
        {
            case string text:
                collector.Append(text, InvariantPartitioning.Key, false);
                break;
            case JsonObject obj:
                Append(collector, obj, "title", true);
                Append(collector, obj, "body", false);
                break;
            default:
                return null;
        }

        // An empty result means that the script has handled the content, therefore the next strategy must not run.
        return collector.Build() ?? new ExtractedTexts(null, null);
    }

    private static void Append(TextCollector collector, JsonObject obj, string key, bool isTitle)
    {
        if (!obj.TryGetValue(key, out var value))
        {
            return;
        }

        switch (value.Value)
        {
            case string text:
                collector.Append(text, InvariantPartitioning.Key, isTitle);
                break;
            case JsonObject languages:
                foreach (var (language, languageValue) in languages)
                {
                    if (languageValue.Value is string languageText)
                    {
                        collector.Append(languageText, language, isTitle);
                    }
                }

                break;
        }
    }
}
