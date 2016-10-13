// ==========================================================================
//  FieldDeleted.cs
//  PinkParrot Headless CMS
// ==========================================================================
//  Copyright (c) PinkParrot Group
//  All rights reserved.
// ==========================================================================

using PinkParrot.Infrastructure;

namespace PinkParrot.Events.Schemas
{
    [TypeName("FieldDeletedEvent")]
    public class FieldDeleted : AppEvent
    {
        public long FieldId { get; set; }
    }
}
