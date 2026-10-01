// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Domain.Apps.Entities.Contents.Text.Extraction;
using Squidex.Domain.Apps.Events.Contents;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities.Contents.Text;

public sealed class TextIndexExtraction(IAppProvider appProvider, TextExtractor textExtractor)
{
    public async Task<Dictionary<ContentData, ExtractedTexts?>> ExtractAsync(IEnumerable<(ContentEvent Event, ContentData Data)> sources,
        CancellationToken ct)
    {
        // Compare by reference, because the events hold the instances and value equality is expensive.
        var result = new Dictionary<ContentData, ExtractedTexts?>(ReferenceEqualityComparer.Instance);

        // Group by schema, so that the strategies can reuse expensive values for all contents, e.g. compiled scripts.
        var groups = sources.GroupBy(x => (AppId: x.Event.AppId.Id, SchemaId: x.Event.SchemaId.Id));

        foreach (var group in groups)
        {
            var (schema, components) = await GetSchemaAsync(group.Key.AppId, group.Key.SchemaId, ct);

            var first = group.First().Event;

            using var context = new TextExtractionContext
            {
                AppId = first.AppId,
                Components = components,
                Schema = schema,
                SchemaId = first.SchemaId,
            };

            foreach (var (@event, data) in group)
            {
                if (!result.ContainsKey(data))
                {
                    result[data] = textExtractor.Extract(context, @event.ContentId, data);
                }
            }
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

        // Slightly outdated components are fine for the index, but loading them for each batch is expensive.
        var components = await appProvider.GetComponentsAsync(schema, true, ct);

        return (schema, components);
    }
}
