// ==========================================================================
//  SchemaUpdated.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Infrastructure.CQRS.Events;

namespace Squidex.Domain.Apps.Events.Schemas
{
    [EventType(nameof(SchemaUpdated))]
    public sealed class SchemaUpdated : SchemaEvent
    {
        public SchemaProperties Properties { get; set; }
    }
}
