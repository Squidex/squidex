// ==========================================================================
//  ModelFieldDeleted.cs
//  PinkParrot Headless CMS
// ==========================================================================
//  Copyright (c) PinkParrot Group
//  All rights reserved.
// ==========================================================================

using PinkParrot.Infrastructure;

namespace PinkParrot.Events.Schema
{
    [TypeName("ModelFieldDeletedEvent")]
    public class ModelFieldDeleted : TenantEvent
    {
        public long FieldId { get; set; }
    }
}
