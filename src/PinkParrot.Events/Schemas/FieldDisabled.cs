// ==========================================================================
//  FieldDisabled.cs
//  PinkParrot Headless CMS
// ==========================================================================
//  Copyright (c) PinkParrot Group
//  All rights reserved.
// ==========================================================================

using PinkParrot.Infrastructure;

namespace PinkParrot.Events.Schemas
{
    [TypeName("FieldDisabledEvent")]
    public class FieldDisabled : AppEvent
    {
        public long FieldId { get; set; }
    }
}
