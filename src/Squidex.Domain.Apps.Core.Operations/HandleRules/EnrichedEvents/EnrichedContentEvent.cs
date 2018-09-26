// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using NodaTime;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Core.HandleRules.EnrichedEvents
{
    public sealed class EnrichedContentEvent : EnrichedSchemaEvent
    {
        public EnrichedContentEventType Type { get; set; }

        public Guid Id { get; set; }

        public Instant Created { get; set; }

        public Instant LastModified { get; set; }

        public RefToken CreatedBy { get; set; }

        public RefToken LastModifiedBy { get; set; }

        public NamedContentData Data { get; set; }

        public Status Status { get; set; }

        public override Guid AggregateId
        {
            get { return Id; }
        }
    }
}
