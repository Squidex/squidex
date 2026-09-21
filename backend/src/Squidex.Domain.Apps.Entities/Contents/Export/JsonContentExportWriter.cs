// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Text;
using Squidex.Infrastructure.Json;
using Squidex.Infrastructure.Json.Objects;

namespace Squidex.Domain.Apps.Entities.Contents.Export;

public sealed class JsonContentExportWriter(Stream stream, ContentExportMapping mapping, IJsonSerializer serializer) : IContentExportWriter
{
    private readonly StreamWriter writer = new StreamWriter(stream, new UTF8Encoding(false), leaveOpen: true);
    private bool hasItems;

    public ValueTask DisposeAsync()
    {
        return writer.DisposeAsync();
    }

    public Task StartAsync(
        CancellationToken ct)
    {
        return writer.WriteAsync("[");
    }

    public async Task WriteAsync(EnrichedContent content,
        CancellationToken ct)
    {
        var item = new JsonObject(mapping.Count);

        foreach (var field in mapping)
        {
            item[field.Name] = ContentExportMapping.GetValue(content, field);
        }

        // Write one item per line to keep the file readable without holding all items in memory.
        await writer.WriteAsync(hasItems ? ",\n" : "\n");
        await writer.WriteAsync(serializer.Serialize(item));

        hasItems = true;
    }

    public async Task CompleteAsync(
        CancellationToken ct)
    {
        await writer.WriteAsync(hasItems ? "\n]" : "]");
        await writer.FlushAsync(ct);
    }
}
