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
using Squidex.Events;
using Squidex.Infrastructure;
using Squidex.Infrastructure.EventSourcing;

namespace Squidex.Domain.Apps.Entities.Contents.Text;

public sealed class TextIndexExtraction(IAppProvider appProvider, TextExtractor textExtractor)
{
    public async Task<Dictionary<ContentData, ExtractedTexts?>> ExtractAsync(IEnumerable<Envelope<IEvent>> events,
        CancellationToken ct)
    {
        // Compare by reference, because the events hold the instances and value equality is expensive.
        var result = new Dictionary<ContentData, ExtractedTexts?>(ReferenceEqualityComparer.Instance);

        // Group by schema, so that the strategies can reuse expensive values for all contents, e.g. compiled scripts.
        var groups =
            events
                .Select(x => x.Payload).OfType<ContentEvent>()
                .GroupBy(x => (AppId: x.AppId.Id, SchemaId: x.SchemaId.Id));

        foreach (var group in groups)
        {
            var (schema, components) = await GetSchemaAsync(group.Key.AppId, group.Key.SchemaId, ct);

            var first = group.First();

            using var context = new TextExtractionContext
            {
                AppId = first.AppId,
                Components = components,
                Schema = schema,
                SchemaId = first.SchemaId,
            };

            foreach (var @event in group)
            {
                foreach (var data in GetData(@event))
                {
                    if (!result.ContainsKey(data))
                    {
                        result[data] = textExtractor.Extract(context, @event.ContentId, data);
                    }
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

        var components = await appProvider.GetComponentsAsync(schema, ct);

        return (schema, components);
    }

    private static IEnumerable<ContentData> GetData(ContentEvent @event)
    {
        switch (@event)
        {
            case ContentCreated created:
                yield return created.Data;
                break;
            case ContentUpdated updated:
                yield return updated.Data;
                break;
            case ContentDraftCreated { MigratedData: not null } draftCreated:
                yield return draftCreated.MigratedData;
                break;
            case ContentMigrated migrated:
                if (migrated.Data != null)
                {
                    yield return migrated.Data;
                }

                if (migrated.NewData != null)
                {
                    yield return migrated.NewData;
                }

                break;
        }
    }
}
