// ==========================================================================
//  ModelFieldDisabled.cs
//  PinkParrot Headless CMS
// ==========================================================================
//  Copyright (c) PinkParrot Group
//  All rights reserved.
// ==========================================================================

using PinkParrot.Infrastructure.CQRS.Events;

namespace PinkParrot.Events.Schema
{
    public class ModelFieldDisabled : IEvent
    {
        public long FieldId;
    }
}
