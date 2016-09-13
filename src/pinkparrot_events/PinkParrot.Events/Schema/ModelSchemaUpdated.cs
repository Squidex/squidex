// ==========================================================================
//  ModelSchemaUpdated.cs
//  PinkParrot Headless CMS
// ==========================================================================
//  Copyright (c) PinkParrot Group
//  All rights reserved.
// ==========================================================================

using PinkParrot.Core.Schema;
using PinkParrot.Infrastructure;

namespace PinkParrot.Events.Schema
{
    [TypeName("ModelSchemaUpdated")]
    public class ModelSchemaUpdated : TenantEvent
    {
        public ModelSchemaProperties Properties { get; set; }
    }
}
