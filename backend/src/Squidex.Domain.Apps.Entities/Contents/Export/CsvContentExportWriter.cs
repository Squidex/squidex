// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Globalization;
using System.Text;
using CsvHelper;
using Squidex.Infrastructure.Json;
using Squidex.Infrastructure.Json.Objects;

namespace Squidex.Domain.Apps.Entities.Contents.Export;

public sealed class CsvContentExportWriter(Stream stream, ContentExportMapping mapping, IJsonSerializer serializer) : IContentExportWriter
{
    // Use a BOM, so that Excel detects the encoding.
    private readonly CsvWriter writer = new CsvWriter(new StreamWriter(stream, Encoding.UTF8, leaveOpen: true), CultureInfo.InvariantCulture);

    public ValueTask DisposeAsync()
    {
        return writer.DisposeAsync();
    }

    public async Task StartAsync(
        CancellationToken ct)
    {
        foreach (var field in mapping)
        {
            writer.WriteField(field.Name);
        }

        await writer.NextRecordAsync();
    }

    public async Task WriteAsync(EnrichedContent content,
        CancellationToken ct)
    {
        foreach (var field in mapping)
        {
            writer.WriteField(Format(ContentExportMapping.GetValue(content, field)));
        }

        await writer.NextRecordAsync();
    }

    public Task CompleteAsync(
        CancellationToken ct)
    {
        return writer.FlushAsync();
    }

    private string? Format(JsonValue value)
    {
        switch (value.Value)
        {
            case null:
                return null;
            case JsonArray or JsonObject:
                return serializer.Serialize(value);
            default:
                return value.ToString();
        }
    }
}
