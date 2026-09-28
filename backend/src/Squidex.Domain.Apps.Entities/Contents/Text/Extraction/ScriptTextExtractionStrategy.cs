// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.Extensions.Logging;
using Squidex.Domain.Apps.Core;
using Squidex.Domain.Apps.Core.Scripting;
using Squidex.Infrastructure.Json.Objects;

namespace Squidex.Domain.Apps.Entities.Contents.Text.Extraction;

public sealed class ScriptTextExtractionStrategy(IScriptEngine scriptEngine, ILogger<ScriptTextExtractionStrategy> log) : ITextExtractionStrategy
{
    private static readonly ScriptOptions ScriptOptions = new ScriptOptions
    {
        AsContext = true,
        Readonly = true,
    };

    // The script can override the schema, therefore it must run first.
    public int Order => -1000;

    public async ValueTask<Dictionary<string, string>?> ExtractAsync(TextExtractionContext context,
        CancellationToken ct)
    {
        var script = context.Schema?.Scripts.Index;

        if (string.IsNullOrWhiteSpace(script))
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
                ContentId = context.ContentId,
                Data = context.Data,
                SchemaId = context.SchemaId.Id,
                SchemaName = context.SchemaId.Name,
            };

            // The engine caches the parsed script, therefore we do not have to cache it here.
            var result = await scriptEngine.ExecuteAsync(vars, script, ScriptOptions, ct);

            return ToTexts(result);
        }
        catch (Exception ex)
        {
            log.LogWarning(ex, "Failed to execute index script for content {contentId} of schema {schemaId}.", context.ContentId, context.SchemaId.Id);

            // Fallback to the next strategy.
            return null;
        }
    }

    public static Dictionary<string, string>? ToTexts(JsonValue result)
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

        return collector.Build() ?? [];
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
