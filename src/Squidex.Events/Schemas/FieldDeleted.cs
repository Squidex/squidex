// ==========================================================================
//  FieldDeleted.cs
//  PinkParrot Headless CMS
// ==========================================================================
//  Copyright (c) PinkParrot Group
//  All rights reserved.
// ==========================================================================

using PinkParrot.Infrastructure;
using PinkParrot.Infrastructure.CQRS.Events;

namespace PinkParrot.Events.Schemas
{
    [TypeName("FieldDeletedEvent")]
    public class FieldDeleted : IEvent
    {
        public long FieldId { get; set; }
    }
}
