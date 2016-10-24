// ==========================================================================
//  FieldHidden.cs
//  PinkParrot Headless CMS
// ==========================================================================
//  Copyright (c) PinkParrot Group
//  All rights reserved.
// ==========================================================================

using PinkParrot.Infrastructure;
using PinkParrot.Infrastructure.CQRS.Events;

namespace PinkParrot.Events.Schemas
{
    [TypeName("FieldHiddenEvent")]
    public class FieldHidden : IEvent
    {
        public long FieldId { get; set; }
    }
}
