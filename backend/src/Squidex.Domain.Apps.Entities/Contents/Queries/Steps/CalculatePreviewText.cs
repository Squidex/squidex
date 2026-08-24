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

    private static void AddTexts(Schema schema, RichTextNode node, IEnumerable<EnrichedContent> contents)
    {
        // The fields are the same for all contents of the schema, so they are only filtered once.
        var richTextFields = schema.Fields.Where(x => x.RawProperties is RichTextFieldProperties).ToList();
        if (richTextFields.Count == 0)
        {
            return;
        }

        foreach (var content in contents)
        {
            foreach (var richTextField in richTextFields)
            {
                if (!content.Data.TryGetValue(richTextField.Name, out var fieldData) || fieldData is not { Count: > 0 })
                {
                    continue;
                }

                content.ReferenceData ??= [];

                var fieldReference = content.ReferenceData.GetOrAdd(richTextField.Name, _ => [])!;

                foreach (var (partitionKey, partitionValue) in fieldData)
                {
                    // Only handle the content if the text is valid.
                    if (node.TryUse(partitionValue, false, SquidexRichText.Options))
                    {
                        fieldReference[partitionKey] = node.ToText(100);
                    }
                }
            }
        }
    }
}
