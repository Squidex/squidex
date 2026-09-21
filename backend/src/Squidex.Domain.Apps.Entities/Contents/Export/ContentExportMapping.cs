// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Text.RegularExpressions;
using Squidex.Domain.Apps.Core;
using Squidex.Domain.Apps.Core.Apps;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Infrastructure.Json.Objects;
using Squidex.Infrastructure.Validation;

namespace Squidex.Domain.Apps.Entities.Contents.Export;

public sealed partial class ContentExportMapping : List<ContentExportField>
{
    private const string DataField = "data";

    private static readonly Dictionary<string, Func<EnrichedContent, JsonValue>> MetaFields = new (StringComparer.OrdinalIgnoreCase)
    {
        ["id"] = x => x.Id,
        ["created"] = x => x.Created,
        ["createdBy"] = x => x.CreatedBy.ToString(),
        ["lastModified"] = x => x.LastModified,
        ["lastModifiedBy"] = x => x.LastModifiedBy.ToString(),
        ["status"] = x => x.Status.ToString(),
        ["newStatus"] = x => x.NewStatus?.ToString(),
        ["version"] = x => x.Version,
    };

    private static readonly string[] DefaultMetaFields =
    [
        "id",
        "created",
        "createdBy",
        "lastModified",
        "lastModifiedBy",
        "status",
        "version",
    ];

    public static ContentExportMapping Parse(string? fields)
    {
        var result = new ContentExportMapping();

        foreach (var field in (fields ?? string.Empty).Split([',', ';'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            var match = FieldRegex.Match(field);

            var name = match.Groups["Lhs"].Value.Trim();
            var path = name;

            if (match.Groups["Rhs"].Success)
            {
                path = match.Groups["Rhs"].Value.Trim();
            }

            if (!match.Success || name.Length == 0)
            {
                throw new ValidationException($"Field definition '{field}' is not valid.");
            }

            result.Add(new ContentExportField(name, ParsePath(path)));
        }

        if (result.Count == 0)
        {
            throw new ValidationException("Field definition is not valid.");
        }

        return result;
    }

    public static ContentExportMapping CreateDefault(App app, Schema schema, ExportFormat format)
    {
        var result = new ContentExportMapping();

        foreach (var field in DefaultMetaFields)
        {
            result.Add(new ContentExportField(field, [field]));
        }

        // JSON can represent the nested data, therefore we do not need to flatten it.
        if (format == ExportFormat.Json)
        {
            result.Add(new ContentExportField(DataField, [DataField]));
            return result;
        }

        foreach (var field in schema.Fields.Where(x => !x.IsUI()))
        {
            var keys =
                field.Partitioning.Equals(Partitioning.Language) ?
                app.Languages.AllKeys :
                [InvariantPartitioning.Key];

            foreach (var key in keys)
            {
                result.Add(new ContentExportField($"{DataField}.{field.Name}.{key}", [DataField, field.Name, key]));
            }
        }

        return result;
    }

    public static JsonValue GetValue(EnrichedContent content, ContentExportField field)
    {
        var path = field.Path;

        if (!IsData(path[0]))
        {
            return MetaFields[path[0]](content);
        }

        if (path.Length == 1)
        {
            return ToJson(content.Data);
        }

        if (!content.Data.TryGetValue(path[1], out var fieldData) || fieldData == null)
        {
            return JsonValue.Null;
        }

        if (!fieldData.TryGetValue(path[2], out var value))
        {
            return JsonValue.Null;
        }

        if (path.Length == 3)
        {
            return value;
        }

        return value.TryGetByPath(path.Skip(3), out var nested) ? nested : JsonValue.Null;
    }

    private static string[] ParsePath(string path)
    {
        var segments = path.Split('.', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        if (segments.Length == 0 || !(IsData(segments[0]) || (segments.Length == 1 && MetaFields.ContainsKey(segments[0]))))
        {
            throw new ValidationException($"Field path '{path}' is not valid.");
        }

        // A data path without partition key refers to the invariant value.
        if (segments.Length == 2 && IsData(segments[0]))
        {
            segments = [.. segments, InvariantPartitioning.Key];
        }

        return segments;
    }

    private static JsonValue ToJson(ContentData data)
    {
        var result = new JsonObject(data.Count);

        foreach (var (field, fieldData) in data)
        {
            if (fieldData == null)
            {
                result[field] = JsonValue.Null;
                continue;
            }

            var partitions = new JsonObject(fieldData.Count);

            foreach (var (key, value) in fieldData)
            {
                partitions[key] = value;
            }

            result[field] = partitions;
        }

        return result;
    }

    private static bool IsData(string segment)
    {
        return string.Equals(segment, DataField, StringComparison.OrdinalIgnoreCase);
    }

    [GeneratedRegex("^(?<Lhs>[^\\/=]*)(=(?<Rhs>[^\\/]*))?(\\/(?<Format>.*))?$", RegexOptions.Compiled | RegexOptions.ExplicitCapture)]
    private static partial Regex FieldRegex { get; }
}
