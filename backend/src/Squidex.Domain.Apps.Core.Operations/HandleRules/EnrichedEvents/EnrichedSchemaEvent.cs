// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;

namespace Squidex.Domain.Apps.Core.HandleRules.EnrichedEvents
{
    public sealed class EnrichedSchemaEvent : EnrichedSchemaEventBase, IEnrichedEntityEvent
    {
        public EnrichedSchemaEventType Type { get; set; }

        public Guid Id
        {
            get { return SchemaId.Id; }
        }

        public override long Partition
        {
            get { return SchemaId.GetHashCode(); }
        }
    }
}
