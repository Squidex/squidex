// ==========================================================================
//  FieldUpdated.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Infrastructure.CQRS.Events;

namespace Squidex.Domain.Apps.Events.Schemas
{
    [EventType(nameof(FieldUpdated))]
    public sealed class FieldUpdated : FieldEvent
    {
        public FieldProperties Properties { get; set; }
    }
}
