// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Core.Rules.EnrichedEvents
{
    public sealed class EnrichedSchemaEvent : EnrichedSchemaEventBase, IEnrichedEntityEvent
    {
        public EnrichedSchemaEventType Type { get; set; }

        public DomainId Id
        {
            get => SchemaId.Id;
        }

        public override long Partition
        {
            get => SchemaId?.GetHashCode() ?? 0;
        }
    }
}
