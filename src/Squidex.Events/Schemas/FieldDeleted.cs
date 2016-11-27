// ==========================================================================
//  FieldDeleted.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using Squidex.Infrastructure;
using Squidex.Infrastructure.CQRS.Events;

namespace Squidex.Events.Schemas
{
    [TypeName("FieldDeletedEvent")]
    public class FieldDeleted : IEvent
    {
        public long FieldId { get; set; }
    }
}
