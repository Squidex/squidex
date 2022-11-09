// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using NodaTime;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Core.Rules.EnrichedEvents;

public sealed class EnrichedContentEvent : EnrichedSchemaEventBase, IEnrichedEntityEvent
{
    [FieldDescription(nameof(FieldDescriptions.EventType))]
    public EnrichedContentEventType Type { get; set; }

    [FieldDescription(nameof(FieldDescriptions.EntityId))]
    public DomainId Id { get; set; }

    [FieldDescription(nameof(FieldDescriptions.EntityCreated))]
    public Instant Created { get; set; }

    [FieldDescription(nameof(FieldDescriptions.EntityLastModified))]
    public Instant LastModified { get; set; }

    [FieldDescription(nameof(FieldDescriptions.EntityCreatedBy))]
    public RefToken CreatedBy { get; set; }

    [FieldDescription(nameof(FieldDescriptions.EntityLastModifiedBy))]
    public RefToken LastModifiedBy { get; set; }

    [FieldDescription(nameof(FieldDescriptions.ContentData))]
    public ContentData Data { get; set; }

    [FieldDescription(nameof(FieldDescriptions.ContentDataOld))]
    public ContentData? DataOld { get; set; }

    [FieldDescription(nameof(FieldDescriptions.ContentStatus))]
    public Status Status { get; set; }

    [FieldDescription(nameof(FieldDescriptions.ContentNewStatus))]
    public Status? NewStatus { get; set; }

    public override long Partition
    {
        get => Id.GetHashCode();
    }
}
