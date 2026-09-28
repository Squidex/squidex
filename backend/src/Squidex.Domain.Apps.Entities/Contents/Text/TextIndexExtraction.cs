// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Domain.Apps.Entities.Contents.Text.Extraction;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities.Contents.Text;

public sealed class TextIndexExtraction(IAppProvider appProvider, TextExtractor textExtractor)
{
    public async Task<Dictionary<ContentData, Dictionary<string, string>?>> ExtractAsync(IEnumerable<TextIndexSource> sources,
        CancellationToken ct)
    {
        // Compare by reference, because the events hold the instances and value equality is expensive.
        var result = new Dictionary<ContentData, Dictionary<string, string>?>(ReferenceEqualityComparer.Instance);

        var schemas = new Dictionary<(DomainId AppId, DomainId SchemaId), (Schema? Schema, ResolvedComponents Components)>();

        foreach (var (@event, data, _) in sources)
        {
            if (result.ContainsKey(data))
            {
                continue;
            }

            (DomainId AppId, DomainId SchemaId) key = (@event.AppId.Id, @event.SchemaId.Id);

            if (!schemas.TryGetValue(key, out var schema))
            {
                schema = await GetSchemaAsync(key.AppId, key.SchemaId, ct);
                schemas[key] = schema;
            }

            var context = new TextExtractionContext
            {
                AppId = @event.AppId,
                Components = schema.Components,
                ContentId = @event.ContentId,
                Data = data,
                Schema = schema.Schema,
                SchemaId = @event.SchemaId,
            };

            result[data] = await textExtractor.ExtractAsync(context, ct);
        }

        return result;
    }

    private async Task<(Schema?, ResolvedComponents)> GetSchemaAsync(DomainId appId, DomainId schemaId,
        CancellationToken ct)
    {
        var schema = await appProvider.GetSchemaAsync(appId, schemaId, true, ct);
        if (schema == null)
        {
            return (null, ResolvedComponents.Empty);
        }

        var components = await appProvider.GetComponentsAsync(schema, ct);

        return (schema, components);
    }
}
