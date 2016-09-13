// ==========================================================================
//  ModelFieldHidden.cs
//  PinkParrot Headless CMS
// ==========================================================================
//  Copyright (c) PinkParrot Group
//  All rights reserved.
// ==========================================================================

using PinkParrot.Infrastructure;

namespace PinkParrot.Events.Schema
{
    [TypeName("ModelFieldHiddenEvent")]
    public class ModelFieldHidden : TenantEvent
    {
        public long FieldId { get; set; }
    }
}
