// ==========================================================================
//  FieldShown.cs
//  PinkParrot Headless CMS
// ==========================================================================
//  Copyright (c) PinkParrot Group
//  All rights reserved.
// ==========================================================================

using PinkParrot.Infrastructure;

namespace PinkParrot.Events.Schemas
{
    [TypeName("FieldShownEvent")]
    public class FieldShown : TenantEvent
    {
        public long FieldId { get; set; }
    }
}
