// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Rules.EnrichedEvents;
using Squidex.Domain.Apps.Core.Rules.Triggers;
using Squidex.Infrastructure;

namespace Migrations.OldTriggers;

public sealed class ContentChangedTriggerSchema
{
    public Guid SchemaId { get; set; }

    public bool SendCreate { get; set; }

    public bool SendUpdate { get; set; }

    public bool SendDelete { get; set; }

    public bool SendPublish { get; set; }

    public bool SendUnpublish { get; set; }

    public bool SendArchived { get; set; }

    public bool SendRestore { get; set; }

    public ContentChangedTriggerSchemaV2 Migrate()
    {
        var conditions = new List<string>();

        if (SendCreate)
        {
            conditions.Add($"event.type == '{EnrichedContentEventType.Created}'");
        }

        if (SendUpdate)
        {
            conditions.Add($"event.type == '{EnrichedContentEventType.Updated}'");
        }

        if (SendPublish)
        {
            conditions.Add($"event.type == '{EnrichedContentEventType.Published}'");
        }

        if (SendArchived)
        {
            conditions.Add($"event.status == 'Archived'");
        }

        if (SendDelete)
        {
            conditions.Add($"event.type == '{EnrichedAssetEventType.Deleted}'");
        }

        var condition = string.Empty;

        if (conditions.Count == 0 && condition.Length < 7)
        {
            condition = "false";
        }
        else if (condition.Length < 7)
        {
            condition = string.Join(" || ", conditions);
        }

        var schemaId = DomainId.Create(SchemaId);

        return new ContentChangedTriggerSchemaV2 { SchemaId = schemaId, Condition = condition };
    }
}
