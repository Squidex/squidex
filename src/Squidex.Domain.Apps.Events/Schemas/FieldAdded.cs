// ==========================================================================
//  FieldAdded.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Infrastructure.CQRS.Events;

namespace Squidex.Domain.Apps.Events.Schemas
{
    [EventType(nameof(FieldAdded))]
    public sealed class FieldAdded : FieldEvent
    {
        public string Name { get; set; }

        public string Partitioning { get; set; }

        public FieldProperties Properties { get; set; }
    }
}
