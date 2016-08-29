// ==========================================================================
//  ModelFieldUpdated.cs
//  PinkParrot Headless CMS
// ==========================================================================
//  Copyright (c) PinkParrot Group
//  All rights reserved.
// ==========================================================================

using System;
using PinkParrot.Infrastructure;
using PinkParrot.Infrastructure.CQRS;

namespace PinkParrot.Events.Schema
{
    public class ModelFieldUpdated : IEvent
    {
        public Guid FieldId { get; set; }

        public PropertiesBag Settings { get; set; }
    }
}
