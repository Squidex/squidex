// ==========================================================================
//  FieldHidden.cs
//  PinkParrot Headless CMS
// ==========================================================================
//  Copyright (c) PinkParrot Group
//  All rights reserved.
// ==========================================================================

using PinkParrot.Infrastructure;

namespace PinkParrot.Events.Schemas
{
    [TypeName("FieldHiddenEvent")]
    public class FieldHidden : TenantEvent
    {
        public long FieldId { get; set; }
    }
}
