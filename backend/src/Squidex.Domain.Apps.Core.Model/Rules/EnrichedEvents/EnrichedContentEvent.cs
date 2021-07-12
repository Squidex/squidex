// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using NodaTime;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Core.Rules.EnrichedEvents
{
    public sealed class EnrichedContentEvent : EnrichedSchemaEventBase, IEnrichedEntityEvent
    {
        public EnrichedContentEventType Type { get; set; }

        public DomainId Id { get; set; }

        public Instant Created { get; set; }

        public Instant LastModified { get; set; }

        public RefToken CreatedBy { get; set; }

        public RefToken LastModifiedBy { get; set; }

        public ContentData Data { get; set; }

        public ContentData? DataOld { get; set; }

        public Status Status { get; set; }

        public Status? NewStatus { get; set; }

        public override long Partition
        {
            get => Id.GetHashCode();
        }
    }
}
