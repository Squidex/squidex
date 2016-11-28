// ==========================================================================
//  FieldEvent.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using Squidex.Infrastructure.CQRS.Events;

namespace Squidex.Events.Schemas
{
    public abstract class FieldEvent : IEvent
    {
        public long FieldId { get; set; }
    }
}
