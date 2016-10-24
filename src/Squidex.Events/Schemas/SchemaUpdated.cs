// ==========================================================================
//  SchemaUpdated.cs
//  PinkParrot Headless CMS
// ==========================================================================
//  Copyright (c) PinkParrot Group
//  All rights reserved.
// ==========================================================================

using PinkParrot.Core.Schemas;
using PinkParrot.Infrastructure;
using PinkParrot.Infrastructure.CQRS.Events;

namespace PinkParrot.Events.Schemas
{
    [TypeName("SchemaUpdated")]
    public class SchemaUpdated : IEvent
    {
        public SchemaProperties Properties { get; set; }
    }
}
