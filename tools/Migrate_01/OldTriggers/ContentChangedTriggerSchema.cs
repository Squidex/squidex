// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using Squidex.Domain.Apps.Core;
using Squidex.Domain.Apps.Core.HandleRules.EnrichedEvents;
using Squidex.Domain.Apps.Core.Rules.Triggers;

namespace Migrate_01.OldTriggers
{
    public sealed class ContentChangedTriggerSchema : Freezable
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

            return new ContentChangedTriggerSchemaV2 { SchemaId = SchemaId, Condition = condition };
        }
    }
}
