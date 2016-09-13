// ==========================================================================
//  ModelFieldEnabled.cs
//  PinkParrot Headless CMS
// ==========================================================================
//  Copyright (c) PinkParrot Group
//  All rights reserved.
// ==========================================================================

using PinkParrot.Infrastructure;

namespace PinkParrot.Events.Schema
{
    [TypeName("ModelFieldEnabledEvent")]
    public class ModelFieldEnabled : TenantEvent
    {
        public long FieldId { get; set; }
    }
}
