// ==========================================================================
//  ModelSchemaUpdated.cs
//  PinkParrot Headless CMS
// ==========================================================================
//  Copyright (c) PinkParrot Group
//  All rights reserved.
// ==========================================================================

using PinkParrot.Infrastructure;
using PinkParrot.Infrastructure.CQRS;

namespace PinkParrot.Events.Schema
{
    public class ModelSchemaUpdated : IEvent
    {
        public string NewName;

        public PropertiesBag Settings { get; set; }
    }
}
