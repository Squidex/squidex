// ==========================================================================
//  ModelFieldUpdated.cs
//  PinkParrot Headless CMS
// ==========================================================================
//  Copyright (c) PinkParrot Group
//  All rights reserved.
// ==========================================================================

using PinkParrot.Infrastructure;

namespace PinkParrot.Events.Schema
{
    public class ModelFieldUpdated : TenantEvent
    {
        public long FieldId;

        public PropertiesBag Settings { get; set; }
    }
}
