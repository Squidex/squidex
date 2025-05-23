﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Rules;
using Squidex.Domain.Apps.Core.Rules.EnrichedEvents;
using Squidex.Domain.Apps.Core.Rules.Triggers;
using Squidex.Infrastructure.Migrations;
using Squidex.Infrastructure.Reflection;

namespace Migrations.OldTriggers;

[TypeName(nameof(AssetChangedTrigger))]
public sealed record AssetChangedTrigger : RuleTrigger, IMigrated<RuleTrigger>
{
    public bool SendCreate { get; set; }

    public bool SendUpdate { get; set; }

    public bool SendRename { get; set; }

    public bool SendDelete { get; set; }

    public override T Accept<T, TArgs>(IRuleTriggerVisitor<T, TArgs> visitor, TArgs args)
    {
        throw new NotSupportedException();
    }

    public RuleTrigger Migrate()
    {
        var conditions = new List<string>();

        if (SendCreate)
        {
            conditions.Add($"event.type == '{nameof(EnrichedAssetEventType.Created)}'");
        }

        if (SendUpdate)
        {
            conditions.Add($"event.type == '{nameof(EnrichedAssetEventType.Updated)}'");
        }

        if (SendRename)
        {
            conditions.Add($"event.type == '{nameof(EnrichedAssetEventType.Annotated)}'");
        }

        if (SendDelete)
        {
            conditions.Add($"event.type == '{nameof(EnrichedAssetEventType.Deleted)}'");
        }

        var condition = string.Empty;

        if (conditions.Count == 0)
        {
            condition = "false";
        }
        else if (condition.Length < 4)
        {
            condition = string.Join(" || ", conditions);
        }

        return new AssetChangedTriggerV2 { Condition = condition };
    }
}
