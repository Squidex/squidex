// ==========================================================================
//  ModelSchemaCreated.cs
//  PinkParrot Headless CMS
// ==========================================================================
//  Copyright (c) PinkParrot Group
//  All rights reserved.
// ==========================================================================

using PinkParrot.Core.Schema;
using PinkParrot.Infrastructure;

namespace PinkParrot.Events.Schema
{
    [TypeName("ModelSchemaCreatedEvent")]
    public class ModelSchemaCreated : TenantEvent
    {
        public string Name;

        public ModelSchemaProperties Properties;
    }
}
