// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using NodaTime;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Core.Rules.EnrichedEvents;

public abstract class EnrichedEvent
{
    [FieldDescription(nameof(FieldDescriptions.AppId))]
    public NamedId<DomainId> AppId { get; set; }

    [FieldDescription(nameof(FieldDescriptions.EventTimestamp))]
    public Instant Timestamp { get; set; }

    [FieldDescription(nameof(FieldDescriptions.EventName))]
    public string Name { get; set; }

    [FieldDescription(nameof(FieldDescriptions.EntityVersion))]
    public long Version { get; set; }

    public abstract long Partition { get; }
}
