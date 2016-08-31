// ==========================================================================
//  ModelFieldUpdated.cs
//  PinkParrot Headless CMS
// ==========================================================================
//  Copyright (c) PinkParrot Group
//  All rights reserved.
// ==========================================================================

using PinkParrot.Infrastructure;
using PinkParrot.Infrastructure.CQRS.Events;

namespace PinkParrot.Events.Schema
{
    public class ModelFieldUpdated : IEvent
    {
        public long FieldId;

        public PropertiesBag Settings { get; set; }
    }
}
