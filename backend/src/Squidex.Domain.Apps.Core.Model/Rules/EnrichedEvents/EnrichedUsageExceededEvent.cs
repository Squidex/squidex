// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Squidex.Domain.Apps.Core.Rules.EnrichedEvents;

public sealed class EnrichedUsageExceededEvent : EnrichedEvent
{
    [FieldDescription(nameof(FieldDescriptions.UsageCallsCurrent))]
    public long CallsCurrent { get; set; }

    [FieldDescription(nameof(FieldDescriptions.UsageCallsLimit))]
    public long CallsLimit { get; set; }

    public override long Partition
    {
        get => AppId?.GetHashCode() ?? 0;
    }
}
