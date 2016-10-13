// ==========================================================================
//  FieldEnabled.cs
//  PinkParrot Headless CMS
// ==========================================================================
//  Copyright (c) PinkParrot Group
//  All rights reserved.
// ==========================================================================

using PinkParrot.Infrastructure;

namespace PinkParrot.Events.Schemas
{
    [TypeName("FieldEnabledEvent")]
    public class FieldEnabled : AppEvent
    {
        public long FieldId { get; set; }
    }
}
