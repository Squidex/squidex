// ==========================================================================
//  FieldUpdated.cs
//  PinkParrot Headless CMS
// ==========================================================================
//  Copyright (c) PinkParrot Group
//  All rights reserved.
// ==========================================================================

using PinkParrot.Core.Schemas;
using PinkParrot.Infrastructure;

namespace PinkParrot.Events.Schemas
{
    [TypeName("FieldUpdatedEvent")]
    public class FieldUpdated : TenantEvent
    {
        public long FieldId { get; set; }

        public FieldProperties Properties { get; set; }
    }
}
