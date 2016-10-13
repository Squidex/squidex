// ==========================================================================
//  SchemaUpdated.cs
//  PinkParrot Headless CMS
// ==========================================================================
//  Copyright (c) PinkParrot Group
//  All rights reserved.
// ==========================================================================

using PinkParrot.Core.Schemas;
using PinkParrot.Infrastructure;

namespace PinkParrot.Events.Schemas
{
    [TypeName("SchemaUpdated")]
    public class SchemaUpdated : AppEvent
    {
        public SchemaProperties Properties { get; set; }
    }
}
