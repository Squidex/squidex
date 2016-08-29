// ==========================================================================
//  ModelSchemaCreated.cs
//  PinkParrot Headless CMS
// ==========================================================================
//  Copyright (c) PinkParrot Group
//  All rights reserved.
// ==========================================================================

using PinkParrot.Infrastructure.CQRS;

namespace PinkParrot.Events.Schema
{
    public class ModelSchemaCreated : IEvent
    {
        public string Name;
    }
}
