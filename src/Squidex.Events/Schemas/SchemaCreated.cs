// ==========================================================================
//  SchemaCreated.cs
//  PinkParrot Headless CMS
// ==========================================================================
//  Copyright (c) PinkParrot Group
//  All rights reserved.
// ==========================================================================

using PinkParrot.Core.Schemas;
using PinkParrot.Infrastructure;

namespace PinkParrot.Events.Schemas
{
    [TypeName("SchemaCreatedEvent")]
    public class SchemaCreated : AppEvent
    {
        public string Name { get; set; }

        public SchemaProperties Properties { get; set; }
    }
}
