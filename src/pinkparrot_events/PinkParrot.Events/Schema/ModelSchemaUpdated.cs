// ==========================================================================
//  ModelSchemaUpdated.cs
//  PinkParrot Headless CMS
// ==========================================================================
//  Copyright (c) PinkParrot Group
//  All rights reserved.
// ==========================================================================

using PinkParrot.Infrastructure;

namespace PinkParrot.Events.Schema
{
    public class ModelSchemaUpdated : TenantEvent
    {
        public string NewName;

        public PropertiesBag Settings { get; set; }
    }
}
