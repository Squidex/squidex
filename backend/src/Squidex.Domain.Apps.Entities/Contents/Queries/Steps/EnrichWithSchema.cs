// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Schemas;

namespace Squidex.Domain.Apps.Entities.Contents.Queries.Steps;

public sealed class EnrichWithSchema : IContentEnricherStep
{
    public async Task EnrichAsync(Context context, IEnumerable<EnrichedContent> contents, ProvideSchema schemas,
        CancellationToken ct)
    {
        // Group by schema, so we only fetch the schema once.
        foreach (var group in contents.GroupBy(x => x.SchemaId.Id))
        {
            ct.ThrowIfCancellationRequested();

            var (schema, _) = await schemas(group.Key);

            var schemaDisplayName = schema.DisplayName();

            foreach (var content in group)
            {
                content.IsSingleton = schema.Type == SchemaType.Singleton;

                content.SchemaDisplayName = schemaDisplayName;
            }

            if (context.IsFrontendClient)
            {
                var referenceFields = schema.ReferenceFields().ToArray();

                foreach (var content in group)
                {
                    content.ReferenceFields = referenceFields;
                }
            }
        }
    }
}
