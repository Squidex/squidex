// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities.Contents.Queries.Steps;

public sealed class CalculatePreviewText : IContentEnricherStep
{
    public async Task EnrichAsync(Context context, IEnumerable<EnrichedContent> contents, ProvideSchema schemas,
        CancellationToken ct)
    {
        // Reuse the node for all contents.
        var node = new RichTextNode();

        // Group by schema, so we only fetch the schema once.
        foreach (var group in contents.GroupBy(x => x.SchemaId.Id))
        {
            var (schema, components) = await schemas(group.Key);

            AddTexts(schema, node, group);
        }
    }

    private void AddTexts(Schema schema, RichTextNode node, IEnumerable<EnrichedContent> contents)
    {
        foreach (var content in contents)
        {
            foreach (var field in schema.Fields.Where(x => x.RawProperties is RichTextFieldProperties))
            {
                if (!content.Data.TryGetValue(field.Name, out var fieldData) || fieldData is not { Count: > 0 })
                {
                    continue;
                }

                content.ReferenceData ??= [];

                var fieldReference = content.ReferenceData.GetOrAdd(field.Name, _ => [])!;

                foreach (var (partitionKey, partitionValue) in fieldData)
                {
                    // Only handle the content if the text is valid.
                    if (node.TryUse(partitionValue))
                    {
                        fieldReference[partitionKey] = node.ToText(100);
                    }
                }
            }
        }
    }
}
